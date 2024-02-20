using CommunityToolkit.Maui.Views;
using DataLibrary;
using System.Diagnostics;
using static CommonNet8.SearchForMusic;
using static AngelHornetLibrary.AhLog;


namespace MauiMediaPlayer
{
    public partial class MainPage : ContentPage
    {

        int count = 10;

        private readonly PlaylistContext _dbContext;

        public MainPage(PlaylistContext dbcontext)
        //public MainPage()
        {
            InitializeComponent();
            _dbContext = dbcontext;

            var task = SearchUserProfileMusic();
            Task.Run(async () =>
            {
                await task;
                LogInfo("SearchUserProfileMusic() Complete.");

                var _playlists = _dbContext.Playlists.ToList();
                Application.Current.MainPage.Dispatcher.Dispatch(() =>
                               TestPlaylist.ItemsSource = _playlists);

                var _songList = _dbContext.Songs.ToList();
                _songList = _songList.OrderBy(s => s.Title, StringComparer.OrdinalIgnoreCase).ToList();
                Application.Current.MainPage.Dispatcher.Dispatch(() =>
                               TestSonglist.ItemsSource = _songList);

                LogInfo("SongsDb Dispatch Complete.");
            });

            {
                TestPlaylist.ItemsSource = _dbContext.Playlists.ToList();
                var _songList = _dbContext.Songs.ToList();
                _songList = _songList.OrderBy(s => s.Title, StringComparer.OrdinalIgnoreCase).ToList();
                TestSonglist.ItemsSource = _songList;
            }
        }

        private void TestSonglist_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var _song = (Song)e.SelectedItem;
            var _index = e.SelectedItemIndex;
            var _listView = (ListView)sender;
            _listView.Focus();

            PlaySong(_song);
        }

        private void mediaElement_MediaEnded(object sender, EventArgs e)
        {
            var _selectedSong = (Song)TestSonglist.SelectedItem;
            var _sourceSongList = TestSonglist.ItemsSource.Cast<Song>().ToList();
            var _selectedSongIndex = _sourceSongList.IndexOf(_selectedSong);
            var _newSong = _sourceSongList.ElementAtOrDefault(_selectedSongIndex + 1);
            var _newSongIndex = _sourceSongList.IndexOf(_newSong);
            var _title = _newSong != null ? _newSong.Title : "null";

            LogInfo($"Changing Songs: ");
            LogInfo($"   From[{_selectedSongIndex}]: {_selectedSong.Title}");
            LogInfo($"     To[{_newSongIndex}]: {_title}");

            if (_newSong != null)
            {
                // PlaySong(_newSong); is not needed here. // The selection action dispatched below triggers the selected event which in turn triggers the PlaySong() method.
                Application.Current.MainPage.Dispatcher.Dispatch(() =>              // We definitely need to use dispatcher here.
                {
                    TestSonglist.SelectedItem = _newSong;
                    TestSonglist.ScrollTo(_newSong, ScrollToPosition.Start, true);
                });
            }
        }



        private void PlaySong(Song song)
        {

            if (song.PathName != null)
            {
                MediaSource _mediaSource = null;
                if (File.Exists(song.PathName)) _mediaSource = MediaSource.FromFile(song.PathName);
                else if (song.PathName.StartsWith("embed://")) _mediaSource = MediaSource.FromResource(song.PathName.Substring(8));
                LogInfo($"PlaySong: _mediaSource = {(_mediaSource == null ? "null" : _mediaSource.ToString())}");
                if (_mediaSource != null)
                {
                    Application.Current.MainPage.Dispatcher.Dispatch(() =>
                    {
                        mediaElement.ShouldAutoPlay = true;
                        mediaTitle.Text = song.Title;
                        mediaArtist.Text = $"{song.Artist}";
                        mediaAlbum.Text = $"{song.Album}";
                        mediaElement.Source = _mediaSource;
                    });
                }
            }
        }
    }
}
