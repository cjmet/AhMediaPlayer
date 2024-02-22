using DataLibrary;
using Microsoft.EntityFrameworkCore;
using static AngelHornetLibrary.AhLog;
using static MauiCli.CliProgramLogic;

namespace MauiCli
{
    internal static class DbProgramLogic
    {


        public static async Task CompleteResetTest()
        {
            DbContextTest(true);
            await CommonNet8.SearchForMusic.SearchUserProfileMusic();
            DbRandomizePlaylists();
            DbReadPlaylists();
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



        public static void DbRandomizePlaylists()
        {
            var _dbContext = new PlaylistContext();
            var playlists = _dbContext.Playlists.ToList();
            var songs = _dbContext.Songs.ToList();

            for (var i = 0; i < 3; i++)
            {
                Playlist playlist = new Playlist()
                {
                    Name = $"Playlist {new Random().Next(0, 99999)}",
                    Description = $"Random Playlist {new Random().Next(0, 99999)}",
                    Songs = new List<Song>()
                };
                foreach (Song song in songs)
                    if (FlipCoin()) playlist.Songs.Add(song);
                LogInfo($"Playlist: {playlist.Name} - {playlist.Description}");
                LogInfo($"  Songs: {playlist.Songs.Count}");
                _dbContext.Playlists.Add(playlist);
                _dbContext.SaveChanges();
            }
            _dbContext.Dispose();
        }

        public static void DbReadPlaylists()
        {
            Console.WriteLine("Reading Playlists ... ");
            var _dbContext = new PlaylistContext();
            var playlists = _dbContext.Playlists.Include(p => p.Songs).ToList();
            var songs = _dbContext.Songs.ToList();
            _dbContext.Dispose();

            Playlist allSongs = new Playlist()
            {
                Name = "All Songs",
                Description = "All Songs",
                Songs = songs
            };
            playlists.Insert(0, allSongs);


            foreach (Playlist playlist in playlists)
            {
                Console.WriteLine("\n");
                PlaylistWriteLine("=== == == === == == ", "=== == == === == == ", "=== == == === == == ", "=== == == === == == ");
                PlaylistWriteLine(playlist.Name, "", playlist.Description, "");
                Console.WriteLine();
                PlaylistWriteLine("Song Title", "Artist", "Album", "Genre");
                PlaylistWriteLine("----------", "------", "-----", "-----");
                foreach (Song song in playlist.Songs.OrderBy(s => s.Title))
                {
                    PlaylistWriteLine(song.Title, song.Artist, song.Album, song.Genre);
                }
                Console.WriteLine();
            }
        }
    }
}