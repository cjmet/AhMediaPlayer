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
            Debug.WriteLine($"AppInfo.PackageName: {AppInfo.PackageName}");
            Debug.WriteLine("\n=== ======================================== ===\n");

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

            {
                Debug.WriteLine("\n\n*** Testing DbContext ***\n");

                var _dbContext = new PlaylistContext();
                Debug.WriteLine($"Win11 DbPath: {_dbContext.DbPath}\n");
                Debug.WriteLine($"ContextId   : {_dbContext.ContextId}");
                _dbContext.Database.EnsureCreated();

                var playlists = _dbContext.Playlists.ToList();
                var songs = _dbContext.Songs.ToList();
                _dbContext.Dispose();

                
                int i = 0;
                foreach (var p in playlists)
                {
                    Debug.WriteLine($"[{++i}] Playlist: {p.Name} - {p.Description}");
                }

                Debug.WriteLine("\n=== =================== ===\n");

                _dbContext = new PlaylistContext();
                foreach (var s in songs)
                {
                    Debug.WriteLine($"[{++i}] Songs: {s.Title} - {s.Comment}");
                }
                foreach (var s in songs)
                {
                    _dbContext.Songs.Remove(s); // cjm
                }
                _dbContext.SaveChanges();
                _dbContext.Dispose();

                _dbContext = new PlaylistContext();
                //_dbContext.Songs.Add(new Song { Title = "Songs Database Erased", Comment = "Bye, Bye, Baby, Goodbye!", PathName = "Eraser.mp3" });
                _dbContext.SaveChanges();

                Debug.WriteLine("\n=== =================== ===\n");
            }

            //return builder.Build();
            return result;
        }
    }
}
