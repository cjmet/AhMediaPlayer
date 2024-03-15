using AhConfig;
using DataLibrary;
using System.Globalization;
using static AngelHornetLibrary.AhLog;

namespace MauiMediaPlayer
{
    public partial class MainPage : ContentPage
    {

        private async void TestSonglist_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var _song = (Song)e.SelectedItem;
            var _index = e.SelectedItemIndex;
            var _listView = (ListView)sender;

            var _list = _listView.ItemsSource.Cast<Song>().ToList();
            if (Const.UseSongCache) await SongCache(_song, _list, _index);
            else PlaySong(_song, _song.PathName);
        }
        private async Task DispatchSonglist(List<Song> _songList)
        {
            // EF Core I Hate you More!
            // Horses Mouth.  Get lists directly from db, use those to make the indices and lists and compare.
            LogDebug($"DispatchSongList[202]");
            if (_songList == null) return;
            var _currentPlaylist = (Playlist)TestPlaylist.SelectedItem;
            int _currentPlaylistId = _currentPlaylist != null ? _currentPlaylist.Id : -1;
            List<int> _playlistIndices = new List<int>();

            if (_currentPlaylist != null && _currentPlaylistId > 1)
            {
                var _dbPlaylist = _dbContext.Playlists.Where(p => p.Id == _currentPlaylistId);
                if (_dbPlaylist != null)
                {
                    var _dbSonglist = _dbPlaylist.SelectMany(p => p.Songs);
                    if (_dbSonglist != null)
                    {
                        _playlistIndices = _dbSonglist.Select(s => s.Id).ToList();
                    }
                }
            }

            foreach (var _song in _songList)
            {
                if (_song == null) continue;
                _song.Star = false;
                if (_song.Id == null) continue;
                if (_playlistIndices.Contains(_song.Id)) _song.Star = true;
            }
            var tmp = _songList.Where(s => s.Star).Count();
            LogDebug($"Dispatch[231]: {tmp} Songs Selected");

            List<vSong> _vSongList = new List<vSong>();
            foreach (var _song in _songList)
            {
                if (_song == null) continue;
                _vSongList.Add(new vSong(_song));
            }

            await this.Dispatcher.DispatchAsync(() =>
            {   // DispatchSonglist()
                TestSonglist.ItemsSource = _vSongList;
            });

        }
        private void FilePathDebug_SizeChanged(object sender, EventArgs e)
        {
            var _label = (Label)sender;

            // cj - This is working, but there HAS to be another better way!
            // Co-Pilot:  _label.SetBinding(Label.TextProperty, new Binding("PathName", source: _label.BindingContext, converter: new HeadTruncateConverter()));  // This might be correct Syntax?!?  Co-Pilot suggested it.
            FilePathWindowWidth = int.Max((int)TestSonglist.Width, Const.AppMinimumWidth);
            var tmp = _label.BindingContext;
            _label.BindingContext = null;
            _label.BindingContext = tmp;
        }

    }

    public class HeadTruncateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (MainPage.FilePathFontSize <= 1) return "";

            var str = value as string;
            var width = MainPage.FilePathWindowWidth;       // I can't find any way to get the width of the control inside here.

            var denom = MainPage.FilePathFontSize / Const.FontSizeDivisor;
            int len = (int)(width / denom);
            return AngelHornetLibrary.AhStrings.HeadTruncate(str, len);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as string;

        }
    }

}
