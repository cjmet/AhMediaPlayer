using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore;
using DataLibrary;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using System.Diagnostics;



namespace MauiMediaPlayer
{
    public static class MauiProgram
    {


        public static MauiApp CreateMauiApp()
        {
            Debug.WriteLine("\n=== ======================================== ===\n");
            Debug.WriteLine($"*** AppInfo.PackageName: {AppInfo.PackageName}");
            Debug.WriteLine("\n=== /AppInfo =============================== ===\n");

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
            Debug.WriteLine("\n=== ======================================== ===\n");
            Debug.WriteLine("*** WARNING: This is a debug build.  The database will be deleted and recreated.  Change this later. (MauiProgram.cs)");
            var _dbContext = new PlaylistContext();
            _dbContext.Database.EnsureDeleted();
            _dbContext.Database.EnsureCreated();
            _dbContext.Playlists.Add(new Playlist { Name = "Default", Description="The Default Play List"});
            _dbContext.Playlists.Add(new Playlist { Name = "Test", Description = "The Test Play List" });
            _dbContext.Playlists.Add(new Playlist { Name = "Test2", Description = "The Test2 Play List" });
            _dbContext.Playlists.Add(new Playlist { Name = "Test3", Description = "The Test3 Play List" });
            _dbContext.SaveChanges();
            _dbContext.Dispose();
            Debug.WriteLine("\n=== /Database Rebuild ====================== ===\n");



            return result;
        }
    }
}
