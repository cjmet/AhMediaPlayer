
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
using CommunityToolkit.Maui.Views;



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
            // 1. Find MP3 Files in ~/Music
            mainMenu.AddItem(new List<string> { "Find MP3 Files in ~/Music" }, () =>
            {
                FindFilesQueueFunc(new List<string>
                    { "c:\\users\\cjmetcalfe\\music", "c:\\users\\khaai\\music" });
            });

            // 2. Find MP3 Files in ~/Music
            mainMenu.AddItem(new List<string> { "Search C:\\" }, () =>
            {
                FindFilesQueueFunc(new List<string>
                    { "C:\\" });
            });

            // 3. Find MP3 Files in ~/Music
            mainMenu.AddItem(new List<string> { "Search M:\\" }, () =>
            {
                FindFilesQueueFunc(new List<string>
                    { "M:\\" });
            });

            // 4. Test MauiMainPage Logic
            mainMenu.AddItem(new List<string> { "MauiMainPage" }, () => { MauiMainPage(); });

            // 5. Read DbContext
            mainMenu.AddItem(new List<string> { "Db Read" }, () => { DbContextTest(false); });

            // 6. Reset DbContext
            mainMenu.AddItem(new List<string> { "Db Reset" }, () => { DbContextTest(true); });

            // 7. Quit
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


            // CallBack
            var _dbContext = new PlaylistContext();
            while (queue.TryDequeue(out result)) {
                {
                    Console.WriteLine(MiddleTruncate(result));
                    Debug.WriteLine(result);

                    var _dirName = Path.GetDirectoryName(result);
                    var _fileName = Path.GetFileNameWithoutExtension(result);
                    _dbContext.Songs.Add(new Song
                    {
                        Title = _fileName,
                        PathName = result,
                        Comment = _dirName
                    });
                }
                _dbContext.SaveChanges();
            }
            _dbContext.Dispose();
            // CallBack

            totalFiles += i;
            Console.WriteLine($"Read [{start.ToString("N0")}]+[{i.ToString("N0")}] = [{totalFiles.ToString("N0")}].");

            if (done) Console.WriteLine("*** FindFiles Completed. ***");
            Debug.WriteLine($"Exiting Callback [{done}");
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
                _dbContext.Songs.Add(new Song { Title = "Test Song 1", Comment = "Comment 1", PathName ="File1.mp3" });
                _dbContext.Songs.Add(new Song { Title = "Test Song 2", Comment = "Comment 1", PathName = "File2.mp3" });
                _dbContext.Songs.Add(new Song { Title = "Test Song 3", Comment = "Comment 1", PathName = "File3.mp3" });
                _dbContext.Songs.Add(new Song { Title = "Test Song 4", Comment = "Comment 1", PathName = "File4.mp3" });
                _dbContext.Songs.Add(new Song { Title = "Test Song 5", Comment = "Comment 1", PathName = "File5.mp3" });
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
                Console.WriteLine($"[{++i}] {s.Title,-30} - {s.PathName,-30} - {s.Comment,-30}");
            }
            Console.WriteLine("...");
            Console.WriteLine("\n\n");

        }




        public static void MauiMainPage()
        {
            var _dbContext = new PlaylistContext();

            //=== ======================================
            //vvv vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

            // Callback for FindFilesConcurrentQueue
            Action<ConcurrentQueue<string>, bool> callback = (ConcurrentQueue<string> queue, bool d) =>
            {


                // CallBack
                string result = "result";
                var _dbContext = new PlaylistContext();
                while (queue.TryDequeue(out result))
                {
                    {
                        Console.WriteLine(MiddleTruncate(result));
                        Debug.WriteLine(result);

                        var _dirName = Path.GetDirectoryName(result);
                        var _fileName = Path.GetFileNameWithoutExtension(result);
                        _dbContext.Songs.Add(new Song
                        {
                            Title = _fileName,
                            PathName = result,
                            Comment = _dirName
                        });
                    }
                    _dbContext.SaveChanges();
                }
                //_dbContext.Dispose();
                // CallBack


                //string r;
                //while (q.TryDequeue(out r))
                //{
                //    Debug.WriteLine($"*** DeQueueing: {r}");
                //    var _dirName = Path.GetDirectoryName(r);
                //    var _fileName = Path.GetFileNameWithoutExtension(r);
                //    Debug.WriteLine($"*** Adding: {r}");
                //    _dbContext.Songs.Add(new Song
                //    {
                //        Title = _fileName,
                //        PathName = r,
                //        Comment = _dirName
                //    });
                //    Debug.WriteLine($"*** Added: {r}");
                //}
                //Debug.WriteLine(" *** Saving Changes: _dbContext ***");
                //_dbContext.SaveChanges();
                //Debug.WriteLine(" *** Saved ***");

                if (_dbContext.Songs.Count() > 0 || true)
                {
                    Debug.WriteLine(" *** _dbContext.Songs.Count() > 0 ***");
                    var _songs = _dbContext.Songs.ToList();
                    _songs = _songs.OrderBy(s => s.Title, StringComparer.OrdinalIgnoreCase).ToList();
                    
                    //Application.Current.MainPage.Dispatcher.Dispatch(() =>
                    //    TestSonglist.ItemsSource = _songs);

                    // cjm - StringComparer.OrdinalIgnoreCase is causing a crash here
                    // changed to query into a tmp List, then sort the tmp List, then set the ItemSource
                    //_dbContext.Songs.OrderBy(s => s.Title));   

                    //Application.Current.MainPage.Dispatcher.Dispatch(() =>
                    //    pathText.Text = $"{_dbContext.Songs.Count().ToString()} Songs Found.");

                    // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

                    if (d || true)
                    {
                        string _filename = _dbContext.Songs.FirstOrDefault().PathName;
                        Debug.WriteLine("\n=== ======================================== ===\n");
                        Debug.WriteLine($"_filename = {_filename}");
                        Debug.WriteLine("\n=== ======================================== ===\n");
                        MediaSource _mediaSource;
                        if (_filename != null && File.Exists(_filename))
                        {
                            _mediaSource = MediaSource.FromFile(_filename);
                            if (_mediaSource != null)
                            {
                                //mediaElement.Source = _mediaSource;
                            }
                        }
                    }
                    // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                }

            };
            // /Callback for FindFilesConcurrentQueue

            {
                List<string> paths = new List<string> { "C:\\users\\cjmetcalfe\\Music", "c:\\users\\khaai\\Music" };
                Debug.WriteLine("\n=== ======================================== ===\n");
                Debug.WriteLine("FindFilesConcurrentQueue ... ");
                foreach (var _path in paths)
                    Debug.WriteLine($"::> {_path}");
                Debug.WriteLine("\n=== ======================================== ===\n");

                new FindFilesConcurrentQueue(callback, paths, "*.mp3", SearchOption.AllDirectories);
            }

            //^^^ ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            //=== ======================================

            _dbContext.Dispose();
        }


    }
}