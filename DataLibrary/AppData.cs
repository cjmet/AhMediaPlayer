using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLibrary
{
    public class AppData
    {
        private static string _appDataPath = "";
        public static string AppDataPath
        {
            get
            {
                if (_appDataPath != "") return _appDataPath;
                Environment.SpecialFolder folder;
                if (Debugger.IsAttached)
                    folder = Environment.SpecialFolder.DesktopDirectory;
                else
                    folder = Environment.SpecialFolder.UserProfile;
                _appDataPath = Environment.GetFolderPath(folder);
                _appDataPath = Path.Join(_appDataPath, "_AhMediaPlayer");
                return _appDataPath;
            }
            private set { _appDataPath = value; }
        }



        private static string _tmpDataPath = "";
        public static string TmpDataPath
        {
            get
            {
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
