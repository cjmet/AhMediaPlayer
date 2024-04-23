using AhConfig;
using DataLibrary;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using static AngelHornetLibrary.AhLog;

namespace AhMediaPlayer
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
        private async Task DispatchSonglist(List<Song> _songList, string? _sortby = null)
        {
            // EF Core I Hate you More!
            // Horses Mouth.  Get lists directly from db, use those to make the indices and lists and compare.

            LogDebug($"DispatchSongList[202]: Songs[{_songList.Count}]   Sortby[{_sortby}]");
            if (_songList == null) return;
            var _currentPlaylist = (Playlist)TestPlaylist.SelectedItem;
            int _currentPlaylistId = _currentPlaylist != null ? _currentPlaylist.Id : -1;

            List<vSong> _vSongList = new List<vSong>();
            var _songListIds = _songList.Select(s => s.Id).ToList();

            List<int> _sortOrderList = new List<int>();  // Current Playlist Sort Order
            List<int> _starsList = new List<int>();      // Songs on the current Playlist but not in the SortOrder
            List<int> _appendList = new List<int>();     // Songs not on the current Playlist

            if (_currentPlaylist?.SongIds != null) _sortOrderList = _currentPlaylist.SongIds.ToList();
            if (_currentPlaylist?.Songs != null) _starsList = _currentPlaylist.Songs.Select(s => s.Id).ToList();    
            LogTrace($"Dispatch[211]: Playlist[{_currentPlaylistId}]   Sort[{_sortOrderList.Count}]   Star[{_starsList.Count}]   Songs[{_songListIds.Count}]");

            _starsList = _starsList.Intersect(_songListIds).ToList();
            _appendList = _songListIds.Except(_starsList).ToList();
            _sortOrderList = _sortOrderList.Intersect(_starsList).ToList();
            _starsList = _starsList.Except(_sortOrderList).ToList();
            

            LogTrace($"Dispatch[221]: Playlist[{_currentPlaylistId}]   Sort[{_sortOrderList.Count}]   Star[{_starsList.Count}]   Append[{_appendList.Count}]");
            foreach (var _id in _sortOrderList)
            {
                var _song = _songList.FirstOrDefault(s => s.Id == _id);
                var _vsong = new vSong(_song);
                _vsong.Star = true;
                _vSongList.Add(_vsong);
            }
            foreach (var id in _starsList)
            {
                var _song = _songList.FirstOrDefault(s => s.Id == id);
                var _vsong = new vSong(_song);
                _vsong.Star = true;
                _vSongList.Add(_vsong);
            }
            foreach (var _id in _appendList)
            {
                var _song = _songList.FirstOrDefault(s => s.Id == _id);
                _vSongList.Add(new vSong(_song));
            }

            // Sort by ... 
            {
                if (_sortby == null)
                {
                    if (Searchby?.SelectedItem?.ToString() != null) _sortby = Searchby.SelectedItem.ToString();
                    else _sortby = "Any";
                }

                if (_sortby == "Title" )
                {
                    _vSongList = _vSongList.OrderBy(s => s.AlphaTitle, StringComparer.OrdinalIgnoreCase).ToList();
                    LogTrace($"Dispatch[241]: Sorting by {_sortby}");
                }
                else if (_sortby == "Artist")
                {
                    _vSongList = _vSongList
                        .OrderBy(s => s.Artist, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(s => s.AlphaTitle, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    LogTrace($"Dispatch[243]: Sorting by {_sortby}");
                }
                else if (_sortby == "Album")
                {
                    _vSongList = _vSongList
                        .OrderBy(s => s.Album, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(s => s.AlphaTitle, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    LogTrace($"Dispatch[245]: Sorting by {_sortby}");
                }
                else if (_sortby == "Genre")
                {
                    _vSongList = _vSongList
                        .OrderBy(s => s.Genre, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(s => s.AlphaTitle, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    LogTrace($"Dispatch[247]: Sorting by {_sortby}");
                }
                else if (_sortby == "Path")
                {
                    _vSongList = _vSongList
                        .OrderBy(s => s.PathName, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(s => s.AlphaTitle, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    LogTrace($"Dispatch[249]: Sorting by {_sortby}");
                }
                else if (_sortby == "Playlist" || _sortby == "Any")  // IS Playlist 
                {
                    LogTrace($"Dispatch[251]: Sorting by {_sortby}");
                }
                else if (_sortby == "Shuffle")
                {
                    LogTrace($"Dispatch[251]: Sorting by {_sortby}");
                }
            }
            // /Sort

            LogDebug($"Dispatching[261]: {_vSongList.Count} Songs");

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
