

// Ignore Spelling: Playlists

using AngelHornetLibrary;
using AngelHornetLibrary.CLI;
using DataLibrary;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.NetworkInformation;
using static AngelHornetLibrary.AhLog;
using static CommonNet8.SearchForMusic;



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
            mainMenu.AddItem(new List<string> { "Find MP3 Files" }, () =>
            {
                _ = SearchUserProfileMusic();
            });

            mainMenu.AddItem(new List<string> { "Read Playlists" }, () => { DbProgramLogic.DbReadPlaylists(); });

            mainMenu.AddItem(new List<string> { "Random Playlists" }, () => { DbProgramLogic.DbRandomizePlaylists(); });

            mainMenu.NewLine();

            mainMenu.AddItem(new List<string> { "Complete Reset Test" }, () => { DbProgramLogic.CompleteResetTest().Wait(); });

            mainMenu.AddItem(new List<string> { "Db Reset" }, () => { DbProgramLogic.DbContextTest(true); });

            mainMenu.AddItem(new List<string> { "Quit", "Exit" }, () => { mainMenu.Exit(); });
            mainMenu.AddDefault(() => { });

            mainMenu.Loop();
            Environment.Exit(0);
        }
    }
}