

namespace AhConfig

{
    public static class Const
    {                                                           // Default: Description
        public const string InternalVersion = "240329";         // N/A  : The internal version of the app.

        public const int  MinimumLogLevel = 2;                  // 2    : 0=Trace, 1=Debug, 2=Information, 3=Warning, 4=Error, 5=Critical
        public const bool SaveDatabase = true;                  // TRUE : Save the database, or Reset the Database.
        public const bool UseSongCache = true;                  // TRUE :
        public const bool ShowSongPath = true;                  // TRUE : Show the song path in the GUI.
        public const bool ApiDemoMode = true;                   // TRUE : Limit the API responses so it doesn't crash the Swagger Page.
        public const bool ApiDenyMusicSearch = true;            // TRUE : Deny unless you are running API ONLY and not the App.
        public const bool ApiDenySongAdmin = true;              // TRUE : Denies adding, removing, and modifying raw song data.


        // *** ************************************             // *** ************************************
        // You probably don't want to change these

        public static readonly string[]                         
            ApiMusicSearchPaths = 
                [@"C:\Users\Public\Music"];                     // N/A  : If enabled above, then API will automatically search these paths 
        public const int ApiDemoMax = 256;                      // 256  : Limit the API to this number of responses.
        public const int SongPathFontSize = 8;                  // 8    : The font size for the song path.
        public const int CacheSize = 10;                        // 10   : The number of songs to cache.
        public const int CacheSizeMax = 400;                    // 400  : The maximum number of songs to cache.
        public const int CacheCleanInterval = 1000;             // 1000 : Check and clean the cache every 15 minutes or so.
        public const string SpinChars = " ▪️";                   // " ▪️" : ⚬ ▪️ • The characters to use for the spinner.
        public const int ClockTick = 250;                       // 250  : Anything 250 to 1000 is good. Over 1000 is laggy.
        //                                                      //      : Definitely don't go less than say 40, aka: 25fps.      
        public static readonly int SongPathFrameHeight = 14;    // 14   : The height of the song path frame.
        //                                                      // If it's too small you'll cause a crash
        //                                                      // 14x20y, 12x19y, 8x14y

        // ***                                                  //                      This is so it will be available to the non-app projects.
        public const string AppName = "ahmediaplayer";          // "ahmediaplayer"    : If you change the app name you'll need to change this too.
        public const string MusicDbName = "ahmediaplayer.db";   // "ahmediaplayer.db": This is the default name of the music database.
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
