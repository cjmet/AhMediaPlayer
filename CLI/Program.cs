
using AngelHornetLibrary;
using AngelHornetLibrary.CLI;
using DataLibrary;
using Microsoft.Extensions.Logging;
using Serilog;
using static CommonNet8.SearchForMusic;
using static AngelHornetLibrary.AhLog;



namespace MauiCli
{
    internal class Program
    {
        
        static void Main(string[] args) // Main 
        {

            // AhLog: Start, Stop, Log, LogDebug, LogInfo, LogInformation, LogTrace, LogWarning, LogCritical
            Log("Hello, Serilog! Log!");

            Log("Testing Log Levels ...");
            LogTrace("Hello, Serilog! Trace!");
            LogDebug("Hello, Serilog! Debug!");
            LogInformation("Hello, Serilog! Information!");
            LogWarning("Hello, Serilog! Warning!");
            LogInfo("Hello, Serilog! Error!");
            LogCritical("Hello, Serilog! Critical!");
            Log("Log Testing Complete.\n");

            CliMenu mainMenu = new CliMenu();
            mainMenu.MenuMaxWidth = 80;
            mainMenu.Message = "\nMain Menu";
            mainMenu.AddItem(new List<string> { "Find MP3 Files in ~/Music" }, () =>
            {
                var task = SearchUserProfileMusic();
            });

            mainMenu.AddItem(new List<string> { "Db Read" }, () => { DbContextTest(false); });

            mainMenu.AddItem(new List<string> { "Db Reset" }, () => { DbContextTest(true); });

            mainMenu.AddItem(new List<string> { "Quit", "Exit" }, () => { mainMenu.Exit(); });
            mainMenu.AddDefault(() => { });

            mainMenu.Loop();
            Environment.Exit(0);
        }



        // ========================================


        public static async Task FindFilesQueueFunc(string path)
        {
            Console.WriteLine($"FindFilesQueueFunc: {path}");
            await foreach (string filename in new AhGetFiles().GetFilesAsync(path, "*.mp3"))
            {
                Console.WriteLine($"Adding[61]: {filename}");
                AddFilenameToSongDb(filename);
            }
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
            if (reSeed) _dbContext.Database.EnsureDeleted();
            _dbContext.Database.EnsureCreated();
            _dbContext.Dispose();

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

            // --- 

            Console.WriteLine("Reading Songs from Db ...");
            _dbContext = new PlaylistContext();
            var songs = _dbContext.Songs.ToList();
            _dbContext.Dispose();
            i = 0;
            foreach (var s in songs)
            {
                Console.WriteLine($"{s.Title,-30} - {s.Artist,-30} - {s.Album,-30} - {s.Genre.ToString()}");
            }
            Console.WriteLine("...");
            Console.WriteLine("\n\n");

        }





    }
}