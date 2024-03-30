using AhConfig;
using AngelHornetLibrary;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using DataLibrary;
using static AngelHornetLibrary.AhLog;
using Serilog;
using Microsoft.Extensions.Logging;



namespace AhMediaPlayer
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {

            AhLog.Start((Serilog.Events.LogEventLevel)Const.MinimumLogLevel);
            LogMsg($"{AppInfo.PackageName}   Win:{AppInfo.Version}   Ver:{Const.InternalVersion}");

            LogMsg(" CreateBuilder");
            var builder = MauiApp.CreateBuilder();
            LogMsg(" Configuring");
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
            LogMsg(" Building");
            var result = builder.Build();

            LogMsg(" Checking and/or Creating Database");
            try
            {
                var _dbContext = new PlaylistContext();

                if (!Const.SaveDatabase)                
                {
                    LogWarning("WARNING: This is a debug build.  The database will be deleted and recreated.  Change this later. (MauiProgram.cs)");
                    _dbContext.Database.EnsureDeleted();
                    LogMsg("Database Deleted");
                }
                var dbCreated = _dbContext.Database.EnsureCreated();
                _dbContext.Dispose();
                if (dbCreated)
                    LogMsg(" Database Created");
                else
                    LogMsg(" Database Checked");
                
            }
            catch (Exception ex)
            {
                LogError($"ERROR: Database Failed to Load");
                LogError($"ERROR[050]: {ex.Message}");
                throw;
            }

            LogMsg(" MauiProgram.cs Complete");
            return result;
        }
    }

}
