using AngelHornetLibrary;
using static AngelHornetLibrary.AhLog;

namespace MauiCli
{
    internal class TestLogging
    {
        // /Main 
        // ========================================


        public static void TestLogs()
        {
            // AhLog: Start, Stop, Log, LogDebug, LogInfo, LogInformation, LogTrace, LogWarning, LogCritical
            LogTrace("Hello, Serilog! Trace!");
            LogDebug("Hello, Serilog! Debug!");
            LogInformation("Hello, Serilog! Information!");
            LogWarning("Hello, Serilog! Warning!");
            LogError("Hello, Serilog! Error!");
            LogCritical("Hello, Serilog! Critical!");
            _LoggingLevel.MinimumLevel = Serilog.Events.LogEventLevel.Information;
        }
    }
}