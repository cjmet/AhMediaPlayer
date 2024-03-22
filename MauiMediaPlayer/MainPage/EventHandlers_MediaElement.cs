using CommunityToolkit.Maui.Views;
using DataLibrary;
using static AngelHornetLibrary.AhLog;

namespace MauiMediaPlayer
{
    public partial class MainPage : ContentPage
    {

        private void mediaElement_MediaEnded(object sender, EventArgs e)
        {
            var _selectedSong = (Song)TestSonglist.SelectedItem;
            var _sourceSongList = TestSonglist.ItemsSource.Cast<Song>().ToList();
            var _selectedSongIndex = _sourceSongList.IndexOf(_selectedSong);
            if (_selectedSongIndex < 0) _selectedSong = new Song { Title = "null" };
            var _newSong = _sourceSongList.ElementAtOrDefault(_selectedSongIndex + 1);
            if (_newSong == null && _repeatPlaylist && _selectedSongIndex >= _sourceSongList.Count - 1)
            {
                LogMsg($"Repeating Playlist");
                _newSong = _sourceSongList.ElementAtOrDefault(0);
            }
            var _newSongIndex = _sourceSongList.IndexOf(_newSong);
            var _title = _newSong != null ? _newSong.Title : "null";

            LogMsg($"ChangeSong:{_selectedSongIndex}->{_newSongIndex}/{_sourceSongList.Count - 1}: {_title}");
            LogDebug($"   From[{_selectedSongIndex}]: {_selectedSong.Title}");
            LogDebug($"     To[{_newSongIndex}]: {_title}");

            if (_newSong != null)
            {
                // The selection action dispatched below triggers the selected event which in turn triggers the PlaySong() method.
                this.Dispatcher.Dispatch(() =>        // We definitely need to use dispatcher here, OR ELSE!
                {
                    TestSonglist.SelectedItem = _newSong;
                    TestSonglist.ScrollTo(_newSong, ScrollToPosition.Start, true);
                });
            }
        }
        private void PlaySong(Song _song, String _cachedPath)
        {
            LogMsg($"PlaySong: {(_song != null ? _song.Title : _song.FileName)}");
            if (_song == null || _song.Title == null)
            {
                LogWarning("WARN[189]: Song or Title is null");
                return;
            }
            if (!File.Exists(_cachedPath))
            {
                LogDebug($"Cached File Not Found: {_cachedPath}");
                _cachedPath = _song.PathName;
            }
            if (!File.Exists(_cachedPath))
            {
                LogWarn($"WARN[197] Fallback File Not Found: {_cachedPath}");
                return;
            }

            if (_song != null && _cachedPath != null)
                new Task(async () =>
                {

                    MediaSource _mediaSource = null;
                    if (File.Exists(_cachedPath)) _mediaSource = MediaSource.FromFile(_cachedPath);
                    else if (File.Exists(_song.PathName)) _mediaSource = MediaSource.FromFile(_song.PathName);
                    else if (_cachedPath.StartsWith("embed://")) _mediaSource = MediaSource.FromResource(_cachedPath.Substring(8));

                    LogDebug($"MediaSource: {(_mediaSource == null ? "null" : _mediaSource.ToString())}");
                    if (_mediaSource != null)
                    {
                        this.Dispatcher.Dispatch(() =>
                        {
                            mediaElement.ShouldAutoPlay = true;
                            mediaTitle.Text = _song.Title;
                            mediaArtist.Text = $"{_song.Artist}";
                            mediaAlbum.Text = $"{_song.Album}";
                            mediaElement.Source = _mediaSource;
                        });
                    }

                }, TaskCreationOptions.LongRunning).Start();
        }
        private async void Shuffle_Clicked(object sender, EventArgs e)
        {
            LogMsg("Shuffle");
            var button = (Button)sender;
            var _songList = TestSonglist.ItemsSource.Cast<Song>().ToArray();
            new Random().Shuffle(_songList);
            var _list = _songList.ToList();
            await DispatchSonglist(_list,"Shuffle");

        }
        private void NextTrack_Clicked(object sender, EventArgs e)
        {
            LogMsg("Next Track");
            var button = (Button)sender;
            var _songList = TestSonglist.ItemsSource.Cast<Song>().ToList();
            var _selectedSong = (Song)TestSonglist.SelectedItem;
            var _selectedSongIndex = _songList.IndexOf(_selectedSong);
            var _newSong = _songList.ElementAtOrDefault(_selectedSongIndex + 1);
            if (_newSong != null)
                this.Dispatcher.Dispatch(() => TestSonglist.SelectedItem = _newSong);
        }
        private void PreviousTrack_Clicked(object sender, EventArgs e)
        {
            LogMsg("Previous Track");
            var button = (Button)sender;
            var _songList = TestSonglist.ItemsSource.Cast<Song>().ToList();
            var _selectedSong = (Song)TestSonglist.SelectedItem;
            var _selectedSongIndex = _songList.IndexOf(_selectedSong);
            var _newSong = _songList.ElementAtOrDefault(_selectedSongIndex - 1);
            if (_newSong != null)
                this.Dispatcher.Dispatch(() => TestSonglist.SelectedItem = _newSong);
        }
        private void RepeatList_Clicked(object sender, EventArgs e)
        {
            LogMsg($"Repeat Playlist: {_repeatPlaylist}");
            _repeatPlaylist = !_repeatPlaylist;
            this.Dispatcher.Dispatch(() =>
            {
                RepeatList.BackgroundColor = _repeatPlaylist ? Color.Parse("LightBlue") : Color.Parse("Transparent");
            });
        }
        private void FirstTrack_Clicked(object sender, EventArgs e)
        {
            LogMsg("First Track");
            var button = (Button)sender;
            var _songList = TestSonglist.ItemsSource.Cast<Song>().ToList();
            var _newSong = _songList.ElementAtOrDefault(0);
            if (_newSong != null)
                this.Dispatcher.Dispatch(() => TestSonglist.SelectedItem = _newSong);
        }
        private void LastTrack_Clicked(object sender, EventArgs e)
        {
            LogMsg("Last Track");
            var button = (Button)sender;
            var _songList = TestSonglist.ItemsSource.Cast<Song>().ToList();
            var _newSong = _songList.ElementAtOrDefault(_songList.Count - 1);
            if (_newSong != null)
                this.Dispatcher.Dispatch(() => TestSonglist.SelectedItem = _newSong);
        }

    }
}
