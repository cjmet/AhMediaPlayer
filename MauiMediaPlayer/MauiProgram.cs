using AhConfig;
using AngelHornetLibrary;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using DataLibrary;
using static AngelHornetLibrary.AhLog;
using Serilog;
using Microsoft.Extensions.Logging;



namespace MauiMediaPlayer
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {

            AhLog.Start((Serilog.Events.LogEventLevel)Const.MinimumLogLevel);
            LogMsg($"\"{AppInfo.PackageName}\"   -   [{AppInfo.Version}]   -    [{Const.InternalVersion}]");

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkitMediaElement()
                .UseMauiApp<App>().UseMauiCommunityToolkitCore()

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
                // *** DEBUG ***
                if (false)                //cj
                // *** DEBUG ***    
                {
                    LogWarning("WARNING: This is a debug build.  The database will be deleted and recreated.  Change this later. (MauiProgram.cs)");
                    _dbContext.Database.EnsureDeleted();
                    LogDebug("Database Deleted");
                }
                _dbContext.Database.EnsureCreated();
                LogDebug("Database Created");
                _dbContext.Dispose();
            }
            catch (Exception ex)
            {
                LogError($"Database Failed to Load");
                LogError($"ERROR[050]: {ex.Message}");
                throw;
            }

            return result;
        }
    }

}
