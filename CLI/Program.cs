
using DataLibrary;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using AngelHornetLibrary;
using AngelHornetLibrary.CLI;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;



namespace MauiCli
{
    internal class Program
    {

        static IServiceProvider CreateServiceCollection()
        {
            //  When using Dependency Injection,  
            return new ServiceCollection()
                .AddDbContext<PlaylistContext>()
                .BuildServiceProvider();
        }


        static void Main(string[] args) // Main 
        {

            CliMenu mainMenu = new CliMenu();
            mainMenu.Message = "\nMain Menu";
            mainMenu.AddItem(new List<string> { "Find MP3 Files in ~/Music" }, () =>
            {
                FindFilesQueueFunc(new List<string>
                    { "c:\\users\\cjmetcalfe\\music", "c:\\users\\khaai\\music" });
            });
            mainMenu.AddItem(new List<string> { "Search C:\\" }, () =>
            {
                FindFilesQueueFunc(new List<string>
                    { "C:\\" });
            });
            mainMenu.AddItem(new List<string> { "Search M:\\" }, () =>
            {
                FindFilesQueueFunc(new List<string>
                    { "M:\\" });
            });
            mainMenu.AddItem(new List<string> { "Db Read" }, () => { DbContextTest(false); });
            mainMenu.AddItem(new List<string> { "Db Reset" }, () => { DbContextTest(true); });
            mainMenu.AddItem(new List<string> { "Quit", "Exit" }, () => { mainMenu.Exit(); });
            mainMenu.AddDefault(() => { });



            mainMenu.Loop();
            Environment.Exit(0);
        }



        // ========================================


        public static void FindFilesQueueFunc(List<string> paths)
        {
            //Action<ConcurrentQueue<string>, bool> _callback = (ConcurrentQueue<string> q, bool d) => { string r; while (q.TryDequeue(out r)) { Console.WriteLine(r); } };
            //new FindFilesConcurrentQueue(_callback, paths, "*.mp3", SearchOption.AllDirectories);

            Action<ConcurrentQueue<string>, bool> _callback = callback;
            if (FindFilesConcurrentQueue.IsRunningOrWaiting()) { Console.WriteLine("A FindFiles Task is already running."); return; }
            Console.WriteLine("*** Starting FindFilesConcurrentQueue ***");
            new FindFilesConcurrentQueue(_callback, paths, "*.mp3", SearchOption.AllDirectories);

            return;
        }


        static int totalFiles = 0;
        public static void callback(ConcurrentQueue<string> queue, bool done = false)
        {
            string result = "result";
            int i = 0;

            int start = totalFiles;
            Console.WriteLine($"Starting at [{start.ToString("N0")}]+[{i.ToString("N0")}] and Attempting to Read Results");
            while (queue.TryDequeue(out result)) { Console.WriteLine(result); i++; }
            totalFiles += i;
            Console.WriteLine($"Read [{start.ToString("N0")}]+[{i.ToString("N0")}] = [{totalFiles.ToString("N0")}].");

            if (done) Console.WriteLine("*** FindFiles Completed. ***");
            Debug.WriteLine($"Exiting Callback [{done}");
            return;
        }



        public static void FindFilesTaskFunc(List<string> paths)
        {
            int spinner = 0;
            FindFilesTask task = new FindFilesTask(paths, "*.mp3", SearchOption.AllDirectories);
            do
            {
                var spin = "|/-\\"[spinner++ % 4];
                var tmp = $"[{task.ResultsList.Count.ToString("N0")}]{spin} {task.StatusString}";
                var bufferWidth = Console.BufferWidth;
                //var bufferWidth = Console.WindowWidth;
                if (tmp.Length > bufferWidth) tmp = tmp.Substring(0, bufferWidth / 2 - 6) + " ... " + tmp.Substring(tmp.Length - bufferWidth / 2, bufferWidth / 2);
                Console.Write("\r" + new string(' ', Console.BufferWidth) + "\r");
                Console.Write($"{tmp}");
                Thread.Sleep(250);
            } while (task.Status == TaskStatus.Running);
            Console.WriteLine("\n\n");
            foreach (var file in task.ResultsList)
            {
                Console.WriteLine(MiddleTruncate(file));
            }
            Console.WriteLine($"\nFound {task.ResultsList.Count.ToString("N0")} files.\n");
            return;
        }



        public static string MiddleTruncate(string input)
        {
            var length = Console.BufferWidth - 1;
            if (input.Length <= length) return input;
            return input.Substring(0, length / 2 - 3) + " ... " + input.Substring(input.Length - length / 2 + 3);
        }



        public static void DbContextTest(bool reSeed)
        {

            Console.WriteLine("DbContext Test");
            //Console.WriteLine("Test Disabled.\n");  return;
            var _dbContext = new PlaylistContext();
            Console.WriteLine($"DbPath: {_dbContext.DbPath}\n");
            Console.WriteLine($"ContextId: {_dbContext.ContextId}");
            _dbContext.Database.EnsureCreated();
            _dbContext.Dispose();

            if (reSeed)
            {
                Console.WriteLine("Adding Playlists ...");
                _dbContext = new PlaylistContext();

                _dbContext.Database.EnsureDeleted();
                _dbContext.Database.EnsureCreated();

                //Environment.Exit(0);

                Playlist playlist = new Playlist();
                playlist.Name = "Test Playlist CLI";
                playlist.Description = "Test Description CLI";
                _dbContext.Playlists.Add(playlist);

                _dbContext.Playlists.Add(new Playlist
                {
                    Name = "Test Playlist 11",
                    Description = "Test Description 11"
                });
                _dbContext.Playlists.Add(new Playlist
                {
                    Name = "Test Playlist 12",
                    Description = "Test Description 12"
                });
                _dbContext.Playlists.Add(new Playlist
                {
                    Name = "Test Playlist 23",
                    Description = "Test Description 23"
                });
                _dbContext.Playlists.Add(new Playlist
                {
                    Name = "Test Playlist 24",
                    Description = "Test Description 24"
                });
                _dbContext.SaveChanges();
                _dbContext.Dispose();
            }

            // --- 

            Console.WriteLine("Reading Playlists from Db ...");
            _dbContext = new PlaylistContext();
            var playlists = _dbContext.Playlists.ToList();
            _dbContext.Dispose();
            int i = 0;
            foreach (var p in playlists)
            {
                Console.WriteLine($"[{++i}] Playlist: {p.Name} - {p.Description}");
            }
            Console.WriteLine("...");
            Console.WriteLine("\n\n");


            if (reSeed)
            {
                Console.WriteLine("Adding Songs ... ");
                _dbContext = new PlaylistContext();
                _dbContext.Songs.Add(new Song { Title = "Test Song 1", Comment = "Comment 1" });
                _dbContext.Songs.Add(new Song { Title = "Test Song 2", Comment = "Comment 1" });
                _dbContext.Songs.Add(new Song { Title = "Test Song 3", Comment = "Comment 1" });
                _dbContext.Songs.Add(new Song { Title = "Test Song 4", Comment = "Comment 1" });
                _dbContext.Songs.Add(new Song { Title = "Test Song 5", Comment = "Comment 1" });
                _dbContext.SaveChanges();
                _dbContext.Dispose();
            }

            Console.WriteLine("Reading Songs from Db ...");
            _dbContext = new PlaylistContext();
            var songs = _dbContext.Songs.ToList();
            _dbContext.Dispose();
            i = 0;
            foreach (var s in songs)
            {
                Console.WriteLine($"[{++i}] Song: {s.Title} - {s.Comment}");
            }
            Console.WriteLine("...");
            Console.WriteLine("\n\n");

        }




    }
}