using DataLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AngelHornetLibrary.AhLog;

namespace MauiMediaPlayer
{
    public class vSong : Song, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // lightweight view compatible .Star property.  Not really MvvM, just one property.
        public vSong()
        {
        }

        public vSong(Song song) 
        {
            this.Id = song.Id;
            this.Playlists = song.Playlists;
            this.PathName = song.PathName;

            this.FileSize = song.FileSize;
            this.Title = song.Title;
            
            this.Artist = song.Artist;
            this.Band = song.Band;
            this.Album = song.Album;

            this.Track = song.Track;
            this.Year = song.Year;            
            this.Genre = song.Genre;

            this.Length = song.Length;
            this.Star = song.Star;

        }

        public bool Star
        {
            get { return base.Star; }
            set
            {
                base.Star = value;
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Star)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Star)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarSymbol)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarColor)));
            }
        }
        public string StarSymbol
        {
            get { return Star ? "★" : "☆"; }
        }
        public string StarColor
        {
            get { return Star ? "#FFE9BA1B" : "LightGray"; }
        }
    }
}

