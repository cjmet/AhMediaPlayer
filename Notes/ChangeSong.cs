using System.ComponentModel;

/* Unmerged change from project 'MauiMediaPlayer (net8.0-windows10.0.19041.0)'
Before:
using System.Linq;
After:
using System.Diagnostics;
using System.Linq;
*/
using System.Windows.Input;
using static AngelHornetLibrary.AhLog;

namespace MauiMediaPlayer
{
    class ChangeSongClass : INotifyPropertyChanged
    {
        static int i = 0;
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand ChangeSongCommand { get; private set; }

        public ChangeSongClass()
        {
            ChangeSongCommand = new Command<string>((string s) => { ChangeSongMethod(s); });
            PropertyChanged = (sender, e) => { };  // cjm - ???
        }

        private void ChangeSongMethod(string s)
        {
            LogInfo(s);
        }
    }

}
