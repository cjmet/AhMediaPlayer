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
            //builder.Services.AddTransient<ISongRepository, SongRepository>();
            builder.Services.AddTransient<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            var result = builder.Build();

            {
                var _dbContext = new PlaylistContext();
                _dbContext.Database.EnsureDeleted();
                _dbContext.Database.EnsureCreated();    
                Playlist playlist = new Playlist();
                playlist.Name = "Test Playlist X";
                playlist.Description = "Test Description X";
                _dbContext.Playlists.Add(playlist);

                _dbContext.Playlists.Add(new Playlist { Name = "Test Playlist 11", 
                    Description = "Test Description 11"
                });
                _dbContext.Playlists.Add(new Playlist { Name = "Test Playlist 12", 
                    Description = "Test Description 12" });
                _dbContext.SaveChanges();
                _dbContext.Dispose();

                // --- 

                _dbContext = new PlaylistContext();
                var playlists = _dbContext.Playlists.ToList();
                _dbContext.Dispose();
                int i = 0; 
                foreach (var p in playlists)
                {
                    Debug.WriteLine($"[{++i}] Playlist: {p.Name} - {p.Description}");
                }   
                Debug.WriteLine("...");

            }


            //return builder.Build();
            return result;
        }
    }
}
