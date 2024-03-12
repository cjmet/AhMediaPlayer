using static AngelHornetLibrary.AhLog;
using System.Collections.ObjectModel;

using DataLibrary;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Playground
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            List<vSong> _vList = new List<vSong>();

            if (true)   // Load from Database
            {
                var _dbContext = new PlaylistContext();

                LogDebug("Loading Saved Database");
                var _songList = _dbContext.Songs.ToList();
                _songList = _songList.OrderBy(s => s.AlphaTitle, StringComparer.OrdinalIgnoreCase).ToList();

                foreach (var _song in _songList)
                {
                    _vList.Add(new vSong()
                    {
                        Id = _song.Id,
                        Playlists = _song.Playlists,  // != null ? _song.Playlists : new List<Playlist>(),
                        PathName = _song.PathName,

                        FileSize = _song.FileSize,
                        Title = _song.Title,
                        
                        Artist = _song.Artist, 
                        Band = _song.Band,
                        Album = _song.Album,
                        
                        Track = _song.Track,
                        Year = _song.Year,
                        Genre = _song.Genre,
                        
                        Length = _song.Length,
                        Star = _song.Star
                    });
                    LogMsg($"_vList: {_song.Id} {_song.Title} {_song.Artist} {_song.Star}");
                }
            } // /Load from Database

            if (false)      // Load Fake Data
            {
                _vList = new List<vSong>(_vList);
                _vList.Add(new vSong() { Id = 0, Title = "Test Title", Artist = "Test Artist" });
                _vList.Add(new vSong() { Id = 1, Title = "Test Title 2", Artist = "Test Artist 2" });
                _vList.Add(new vSong() { Id = 2, Title = "Test Title 3", Artist = "Test Artist 3" });
                _vList.Add(new vSong() { Id = 3, Title = "Test Title 4", Artist = "Test Artist 4" });
                _vList.Add(new vSong() { Id = 4, Title = "Test Title 5", Artist = "Test Artist 5" });
            }       // /Load Fake Data



            //ItemsListView.SetBinding(ListView.ItemsSourceProperty, new Binding(".", source: _vList));   // cjm *** SetBinding() ***
            ItemsListView.ItemsSource = _vList;     // cjm - this works too, and is much simpler.



            // Double Tap
            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.NumberOfTapsRequired = 2;
            tapGestureRecognizer.Tapped += (sender, args) =>
            {
                var _sender = sender as ListView;
                if (false)
                {
                    foreach (var properties in _sender.GetType().GetProperties())
                    {
                        LogMsg($"_sender: {properties.Name} = {properties.GetValue(_sender)}");
                    }
                    foreach (var properties in args.GetType().GetProperties())
                    {
                        LogMsg($"args: {properties.Name} = {properties.GetValue(args)}");
                    }
                }
                var _vsong = _sender.SelectedItem as vSong;
                _vsong.Star = !_vsong.Star;
                LogMsg("Double Tapped!");
            };
            ItemsListView.GestureRecognizers.Add(tapGestureRecognizer);
            // /Double Tap

        }

        private void OnAddItemClicked(object sender, EventArgs e)
        {
            LogMsg("Add Item Clicked");


        }

        private async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            LogMsg("Item Selected");
            var _sender = (ListView)sender;
            var _vsong = (vSong)e.SelectedItem;
            _vsong.Star = !_vsong.Star;
            LogMsg($"_vsong.Star = {_vsong.Star}");
        }

        private void OnStarClicked(object sender, EventArgs e)
        {
            LogMsg("Star Clicked");
            var _sender = (Button)sender;
            var _vsong = (vSong)_sender.BindingContext;
            _vsong.Star = !_vsong.Star;
            LogMsg($"_vsong.Star = {_vsong.Star}");
        }
    }

}
