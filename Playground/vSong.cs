using DataLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AngelHornetLibrary.AhLog;

namespace Playground
{
    public class vSong : Song, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public bool Star
        {
            get { return base.Star; }
            set
            {
                base.Star = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Star)));
            }
        }
    }
}

