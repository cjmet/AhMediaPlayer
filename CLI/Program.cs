

// Ignore Spelling: Playlists

using AngelHornetLibrary;
using AngelHornetLibrary.CLI;
using DataLibrary;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.NetworkInformation;
using static AngelHornetLibrary.AhLog;
using static CommonNet8.SearchForMusic;
using static CommonNet8.AllSongsPlaylist;
using static MauiCli.DbProgramLogic;



namespace MauiCli
{
    internal class Program : TestLogging
    {
        // ========================================
        // Main 
        static void Main(string[] args) 
        {
            TestLogs();

            CliMenu mainMenu = new CliMenu();
            mainMenu.MenuMaxWidth = 80;
            mainMenu.MenuItemWidth = 40;
            mainMenu.Message = "\nMain Menu";

            mainMenu.AddItem("Find MP3 Files", () => { _ = SearchUserProfileMusic();  });
            mainMenu.AddItem("Read Playlists", () => { DbReadPlaylists().Wait(); });
            mainMenu.AddItem("Update All Songs Playlist", () => { _ = UpdateAllSongsPlaylist(); });
            mainMenu.AddItem("Random Playlists", () => { DbRandomizePlaylists(); });

            mainMenu.AddItem("Complete Reset Test", () => { CompleteResetTest().Wait(); });
            mainMenu.AddItem("Db Reset", () => { DbContextTest(true); });
            mainMenu.AddItem(new List<string> { "Quit", "Exit" }, () => { mainMenu.Exit(); });
            mainMenu.AddDefault(() => { });

            mainMenu.Loop();
            Environment.Exit(0);
        }
    }
}