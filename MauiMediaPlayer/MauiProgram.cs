using AngelHornetLibrary;
using CommunityToolkit.Maui;
using DataLibrary;
using static AngelHornetLibrary.AhLog;
using System.Diagnostics;
using Microsoft.Extensions.Logging;



namespace MauiMediaPlayer
{
    public static class MauiProgram
    {


        public static MauiApp CreateMauiApp()
        {
#if DEBUG            
            LogDebug($"{AppInfo.PackageName}");
#else
            LogMsg($"{AppInfo.PackageName}");
#endif

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

            try
            {
                var _dbContext = new PlaylistContext();
                LogMsg($"DbPath: {_dbContext.DbPath}");
                _dbContext.Database.EnsureCreated();
                _dbContext.Dispose();
            }
            catch (Exception ex)
            {
                LogError($"Database Failed to Load");
                LogError($"ERROR[050]: {ex.Message}");
                throw ex;
            }

            // For Debugging
            if (false)
            {
                LogDebug("=== ======================================== ===");
                LogDebug("*** WARNING: This is a debug build.  The database will be deleted and recreated.  Change this later. (MauiProgram.cs)");
                var _dbContext = new PlaylistContext();
                _dbContext.Database.EnsureDeleted();
                _dbContext.Database.EnsureCreated();
            }

            return result;
        }
    }

}
