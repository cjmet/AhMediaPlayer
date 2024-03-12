using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using DataLibrary;
using static AngelHornetLibrary.AhLog;

namespace Playground
{
    public class SongViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Song> Songs { get; set; } = new ObservableCollection<Song>();
        public event PropertyChangedEventHandler PropertyChanged;

        public void UpdateStar(int songId)
        {
            var song = Songs.FirstOrDefault(s => s.Id == songId);
            if (song != null)
            {
                LogMsg($"Updating Star: [{song.Star}] {song.Title}");
                song.Star = !song.Star;
                song.Band = song.Star ? "Star" : "Moon";
                LogMsg($"Property Changed: [{song.Star}] {song.Title}");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Songs)));
            }
        }
    }
}
