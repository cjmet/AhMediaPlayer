using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore;
using DataLibrary;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;



namespace MauiMediaPlayer
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
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

            {
                var _dbContext = new PlaylistContext();
                _dbContext.ResetDb();      
                _dbContext.Playlists.Add(new Playlist { Name = "Test Playlist 11", 
                    Description = "Test Description 11"
                });
                _dbContext.Playlists.Add(new Playlist { Name = "Test Playlist 12", 
                    Description = "Test Description 12" });
                _dbContext.SaveChanges();
                _dbContext.Dispose();
                //dbContext.Database.Dispose(); // Dispose of the database connection
            }

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
