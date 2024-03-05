

namespace AhConfig

{
    public static class Const
    {
        public const int MinimumLogLevel = 2;  // 0=Trace, 1=Debug, 2=Information, 3=Warning, 4=Error, 5=Critical
        public const bool UseSongCache = true;
        public const int ClockTick = 250;

        public const int CacheSize = 10;
        public const int CacheSizeMax = 400;
        public const int CacheCleanInterval = 1000;  



        // *** ************************************
        // You probably don't want to change these
        // *** 
        public const string AppName = "ahmediaplayer";   // This is so it will be available to the non-app projects
        
        public const int AppHeight = 720;
        public const int AppWidth = 415;
        public const int AppDisplayBorder = 48;     // This is ~ Taskbar Height and the Titlebar Height.  There is probably a better way to do this.
        public const int AppMinimumWidth = 415;
        public const int AppMinimumHeight = 415;
        public const int AppMaximumWidth = 755;

        public const double FontSizeDivisor = 1.85;     // Larger is more overall width, smaller is less overall width.
                                                        // 1.85 worked perfect for 12pt font.  2.00 seems to work better for 8pt.
        public const int TimeOut = 300;
        public const int TimeOutShort = 30;

        public const int VRB = 1000;        // display delays for each log level
        public const int DBG = 1500;
        public const int INF = 3000;
        public const int WRN = 8000;
        public const int ERR = 12000;
        public const int FTL = 15000;

    }
}
