
namespace CommonNet8
{
    public static class Const
    {
        public const int MinimumLogLevel = 1;  // 0=Trace, 1=Debug, 2=Information, 3=Warning, 4=Error, 5=Critical
        public const bool SearchMireille = false;
        public const bool UseSongCache = true;
        
        public const int AppHeight = 720;
        public const int AppWidth = 405;
        public const int AppDisplayBorder = 48;     // This is ~ Taskbar Height and the Titlebar Height.  There is probably a better way to do this.
        public const int AppMinimumWidth = 385;
        public const int AppMinimumHeight = 415;
        public const int AppMaximumWidth = 755;

        public const double FontSizeDivisor = 1.85;     // Larger is more overall width, smaller is less overall width.
                                                        // 1.85 worked perfect for 12pt font.  2.00 seems to work better for 8pt.
        public const int QueueSize = 20;
        public const int TimeOut = 300;
        public const int TimeOutShort = 30;
    }
}
