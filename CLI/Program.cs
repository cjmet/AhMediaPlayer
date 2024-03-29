﻿

// Ignore Spelling: Playlists

using AngelHornetLibrary;
using AngelHornetLibrary.CLI;
using static CommonNet8.SearchForMusicFiles;
using static CommonNet8.AllSongsPlaylist;
using static MauiCli.DbProgramLogic;
using CommonNet8;
using DataLibrary;



namespace MauiCli
{
    internal class Program : TestLogging
    {
        // ========================================
        // Main 
        static void Main(string[] args) 
        {
            TestLogs();
            PlaylistContext _dbContext = new PlaylistContext();
            
            var progress = new ReportProgressToQueue(new System.Collections.Concurrent.ConcurrentQueue<string>());

            CliMenu mainMenu = new CliMenu();
            mainMenu.MenuMaxWidth = 80;
            mainMenu.MenuItemWidth = 40;
            mainMenu.Message = "\nMain Menu";

            mainMenu.AddItem("Find MP3 Files", () => { _ = SearchUserProfileMusic(_dbContext, progress);  });
            mainMenu.AddItem("Read Playlists", () => { DbReadPlaylists(_dbContext).Wait(); });
            mainMenu.AddItem("Update All Songs Playlist", () => { _ = UpdateAllSongsPlaylist(_dbContext); });
            mainMenu.AddItem("Random Playlists", () => { DbRandomizePlaylists(); });

            mainMenu.AddItem("Complete Reset Test", () => { CompleteResetTest(_dbContext).Wait(); });
            mainMenu.AddItem("Db Reset", () => { DbContextTest(true); });
            mainMenu.AddItem(new List<string> { "Quit", "Exit" }, () => { mainMenu.Exit(); });
            mainMenu.AddDefault(() => { });

            mainMenu.Loop();
            Environment.Exit(0);
        }
    }
}