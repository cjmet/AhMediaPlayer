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
            Log("Hello, Serilog! Log!");

            Log("Testing Log Levels ...");
            LogTrace("Hello, Serilog! Trace!");
            LogDebug("Hello, Serilog! Debug!");
            LogInformation("Hello, Serilog! Information!");
            LogWarning("Hello, Serilog! Warning!");
            LogInfo("Hello, Serilog! Error!");
            LogCritical("Hello, Serilog! Critical!");
            Log("Log Testing Complete.\n");
        }
    }
}