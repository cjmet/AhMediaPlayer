using CommunityToolkit.Maui;
using DataLibrary;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using static AngelHornetLibrary.AhLog;



namespace MauiMediaPlayer
{
    public static class MauiProgram
    {


        public static MauiApp CreateMauiApp()
        {
            LogInfo("\n=== ======================================== ===\n");
            LogInfo($"*** AppInfo.PackageName: {AppInfo.PackageName}");
            LogInfo("\n=== /AppInfo =============================== ===\n");

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkitMediaElement()

                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddDbContext<PlaylistContext>();
            builder.Services.AddTransient<IPlaylistRepository, PlaylistRepository>();
            builder.Services.AddTransient<ISongRepository, SongRepository>();
            builder.Services.AddTransient<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            var result = builder.Build();



            // Add Stuff Here Later // - cjm 
            if (true)
            {
                LogInfo("\n=== ======================================== ===\n");
                LogInfo("*** WARNING: This is a debug build.  The database will be deleted and recreated.  Change this later. (MauiProgram.cs)");
                var _dbContext = new PlaylistContext();
                _dbContext.Database.EnsureDeleted();
                _dbContext.Database.EnsureCreated();
                _dbContext.Playlists.Add(new Playlist { Name = "Default", Description = "The Default Play List" });
                _dbContext.Playlists.Add(new Playlist { Name = "Test", Description = "The Test Play List" });
                _dbContext.Playlists.Add(new Playlist { Name = "Test2", Description = "The Test2 Play List" });
                _dbContext.Playlists.Add(new Playlist { Name = "Test3", Description = "The Test3 Play List" });

                _dbContext.Songs.Add(new Song
                {
                    Title = "B - Test Song - With a Very Long N a m e - and Big Long Band Name",
                    Artist = "Test Artist - WithaVeryLongName - And Thirty Seven Kids",
                    Album = "Test Album  - With a Very Long Name",
                    Genre = "Test Genre - With a Very Long Name - And a Whole Bunch More Gibberish",
                    PathName = "embed://gs-16b-1c-44100hz.mp3"
                });
                _dbContext.Songs.Add(new Song
                {
                    Title = "B - Test Song",
                    Artist = "Test Artist",
                    Album = "Test Album",
                    Genre = "Test Genre",
                    PathName = "embed://gs-16b-1c-44100hz.mp3"
                });

                _dbContext.SaveChanges();
                _dbContext.Dispose();
                LogInfo("\n=== /Database Rebuild ====================== ===\n");
            }


            return result;
        }
    }
}
