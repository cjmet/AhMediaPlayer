

namespace AhConfig

{
    public static class Const
    {                                                           // Default: Description
        public const string InternalVersion = "240314";         // N/A  : The internal version of the app.
        public const int MinimumLogLevel = 2;                   // 2    : 0=Trace, 1=Debug, 2=Information, 3=Warning, 4=Error, 5=Critical
        public const bool UseSongCache = true;                  // TRUE :
        public const int SongPathFontSize = 8;                  // 8    : The font size for the song path.

        public const bool ApiAllowSongAdmin = false;            // FALSE: Allows adding, removing, and modifying raw song data.
        public const bool ApiDemoMode = true;                   // TRUE : Limit the API responses so it doesn't crash the Swagger Page.
        public const int ApiDemoMax = 256;                      // 256  : Limit the API to this number of responses.
        public const bool ApiAllowMusicSearch = false;          // FALSE: You should NOT do this, unless you are running API ONLY and not the App.
        public static readonly string[]                         
            ApiMusicSearchPaths = 
                [@"C:\Users\Public\Music"];                     // N/A  : If enabled above API will automatically search %userprofile%\Music

        public const int CacheSize = 10;                        // 10   : The number of songs to cache.
        public const int CacheSizeMax = 400;                    // 400  : The maximum number of songs to cache.
        public const int CacheCleanInterval = 1000;             // 1000 : Check and clean the cache every 15 minutes or so.

        public const string SpinChars = " ▪️";                   // " ▪️" : ⚬ ▪️ • The characters to use for the spinner.
        public const int ClockTick = 250;                       // 250  : Anything 250 to 1000 is good. Over 1000 is laggy.
                                                                //      : Definitely don't go less than say 40, aka: 25fps.      
        public static readonly int SongPathFrameHeight = 
                     (int)(1.5 + SongPathFontSize * 1.5);       // ~=13 : The height of the song path frame. ~= 1 + 1.5 * SongPathFontSize.  "+1" If it's not high enough it will cause a crash.

        // *** ************************************
        // You probably don't want to change these
        // ***                                                  //                      This is so it will be available to the non-app projects.
        public const string AppName = "ahmediaplayer";          // "ahmediaplayer"    : If you change the app name you'll need to change this too.
        public const string MusicDbName = "test_playlists.db";  // "test_playlists.db": This is the default name of the music database.
        public const string ApiIdDbName = "ApiIdDb.db";         // "ApiIdDb.db"       : This is the default name of the API Identity database.

        public const int AppHeight = 720;                       // 720  : This is the default height of the app.
        public const int AppWidth = 415;                        // 415  : This is the default width of the app.
        public const int AppDisplayBorder = 48;                 // 48   : This is ~ Taskbar Height and the Titlebar Height.  There is probably a better way to do this.
        public const int AppMinimumWidth = 415;                 // 415  : This is the minimum width of the app.
        public const int AppMinimumHeight = 415;                // 415  : This is the minimum height of the app.
        public const int AppMaximumWidth = 755;                 // 755  : This is the maximum width of the app.
        public const double FontSizeDivisor = 1.85;             // 1.85 : Larger is more overall width, smaller is less overall width.
                                                                //        1.85 worked perfect for 12pt font.  2.00 seems to work better for 8pt.

        public const int TimeOut = 300;                         // 300  : ... 
        public const int TimeOutShort = 30;                     // 30   : ....
        
        public const int VRB = 1000;                            // 1000 : App Message Box Display Duration and Priority based on Log level.
        public const int DBG = 1500;                            // 1500 : ...
        public const int INF = 3000;                            // 3000 : ...
        public const int WRN = 8000;                            // 8000 : ...
        public const int ERR = 12000;                           // 12000: ...
        public const int FTL = 15000;                           // 15000: ...

    }
}
