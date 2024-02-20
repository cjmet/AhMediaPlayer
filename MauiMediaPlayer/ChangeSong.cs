using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MauiMediaPlayer;
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
