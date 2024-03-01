using System.Diagnostics;
using static AngelHornetLibrary.AhLog;

namespace AhConfig
{
    public class AppData
    {
        // 
        private static string _appDataPath = "";
        public static string AppDataPath
        {
            get
            {
                if (_appDataPath != "") return _appDataPath;
                LogDebug("Initializing AppDataPath");
                Environment.SpecialFolder folder;
                folder = Environment.SpecialFolder.LocalApplicationData;
                _appDataPath = Environment.GetFolderPath(folder);
                var _throwback = _appDataPath;
                var _fallback = Path.Join(_appDataPath, "_AhMediaPlayer");
                
                _appDataPath = Path.Join(_appDataPath, "Packages" );
                var appDirectory = Directory.GetDirectories(_appDataPath, $"*{Const.AppName}*");
                if (appDirectory.Length > 0)
                {
                    foreach (var t in appDirectory) LogTrace($"Found: {t}");
                    _appDataPath = appDirectory[0];
                    _appDataPath = Path.Join(_appDataPath, "LocalCache");
                    _appDataPath = Path.Join(_appDataPath, "Local");
                    _appDataPath = Path.Join(_appDataPath, "_AhMediaPlayer");
                }
                else
                {
                    LogWarn($"WARN[034]: Package Directory Not Found for {Const.AppName}");
                    _appDataPath = _fallback;
                }
                var isOK = false;
                try {
                    LogTrace($"Trying: {_appDataPath}");
                    Directory.CreateDirectory(_appDataPath); 
                    isOK = true;
                }
                catch (Exception ex)
                {
                    LogError("ERROR[039]: creating AppDataPath ");
                    LogError(_appDataPath);
                    LogError(ex.Message);
                    _appDataPath = _fallback;
                }
                //if (!isOK)
                {
                    try
                    {
                        LogTrace($"Trying: {_appDataPath}");
                        Directory.CreateDirectory(_appDataPath);
                    }
                    catch (Exception ex)
                    {
                        LogError("ERROR[047]: creating AppDataPath ");
                        LogError(_appDataPath);
                        LogError(ex.Message);
                        _appDataPath = _throwback;
                    }
                }

                LogDebug("AppDataPath: " + _appDataPath);
                return _appDataPath;
            }
            private set { _appDataPath = value; }
        }



        private static string _tmpDataPath = "";
        public static string TmpDataPath
        {
            get
            {
                // I changed my mind on how to do this and where to put it.  
                // We need a place we can control so we can harmlessly delete out files as it gets too big.
                //if (_tmpDataPath != "") return _tmpDataPath;
                //var folder = Environment.SpecialFolder.InternetCache;
                //_tmpDataPath = Environment.GetFolderPath(folder);
                //_tmpDataPath = Path.Join(_tmpDataPath, "_AhMediaPlayer");  // We aren't allowed to access a sub-folder here either!!!   Windows FileSystem Virtualization is a PITA!
                //return _tmpDataPath;
                return AppDataPath;
            }
            private set { _tmpDataPath = value; }
        }
        
    }
}
