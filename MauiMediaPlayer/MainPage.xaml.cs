using CommunityToolkit.Maui.Views;
using DataLibrary;
using System.Windows.Input;
using static AngelHornetLibrary.AhLog;
using static CommonNet8.SearchForMusic;
using static CommonNet8.AllSongsPlaylist;




namespace MauiMediaPlayer
{
    public partial class MainPage : ContentPage
    {

        private readonly PlaylistContext _dbContext;
        public string _message = "Angel Hornet Media Player";



        public MainPage(PlaylistContext dbcontext)
        //public MainPage()
        {
            // Change: Application.Current.MainPage.Dispatcher.Dispatch
            // to: this.Dispatcher.Dispatch

            InitializeComponent();
            _dbContext = dbcontext;

            var _ = SecondWindow(Application.Current, TestSonglist);

            // Load Databases
            {
                LogInfo("Loading Saved Database");
                var _playlists = _dbContext.Playlists.ToList();
                _playlists = _playlists.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
                TestPlaylist.ItemsSource = _playlists;
                var _songList = _dbContext.Songs.ToList();
                _songList = _songList.OrderBy(s => s.Title, StringComparer.OrdinalIgnoreCase).ToList();
                TestSonglist.ItemsSource = _songList;
                LogInfo($"Loaded {_playlists.Count} Playlists, and {_songList.Count} Songs.");
                LogInfo("=== /Database Loading Complete =============================== ===");
            }

            // Search for More Music
            Task.Run(async () =>
            {
                await SearchUserProfileMusic(); 
                await UpdateAllSongsPlaylist();

                LogInfo("Database Dispatch Start.");
                var _playlists = _dbContext.Playlists.ToList();
                _playlists = _playlists.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
                this.Dispatcher.Dispatch(() =>
                               TestPlaylist.ItemsSource = _playlists);

                var _songList = _dbContext.Songs.ToList();
                _songList = _songList.OrderBy(s => s.Title, StringComparer.OrdinalIgnoreCase).ToList();
                this.Dispatcher.Dispatch(() =>
                               TestSonglist.ItemsSource = _songList);
                await Task.Delay(1);
                LogInfo("=== /Database Dispatch Complete =============================== ===");
            });

        }


        private void TestPlaylist_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var _playList = (Playlist)e.SelectedItem;
            var _index = e.SelectedItemIndex;
            var _listView = (ListView)sender;
            _listView.Focus();

            if (_playList != null)
            {
                LogInfo($"Playlist Selected: {_playList.Id}   {_playList.Name}   {_playList.Description}");
                ChangePlaylist(_playList);
            }
            else LogWarning("Warning: Playlist Selected is null");
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
            if (_selectedSongIndex < 0) _selectedSong = new Song { Title = "null" };
            var _newSong = _sourceSongList.ElementAtOrDefault(_selectedSongIndex + 1);
            var _newSongIndex = _sourceSongList.IndexOf(_newSong);
            var _title = _newSong != null ? _newSong.Title : "null";

            LogInfo($"Changing Songs: ");
            LogInfo($"   From[{_selectedSongIndex}]: {_selectedSong.Title}");
            LogInfo($"     To[{_newSongIndex}]: {_title}");

            if (_newSong != null)
            {
                // PlaySong(_newSong); is not needed here. // The selection action dispatched below triggers the selected event which in turn triggers the PlaySong() method.
                this.Dispatcher.Dispatch(() =>              // We definitely need to use dispatcher here.
                {
                    TestSonglist.SelectedItem = _newSong;
                    TestSonglist.ScrollTo(_newSong, ScrollToPosition.Start, true);
                });
            }
        }



        private void ChangePlaylist(Playlist playlist)
        {
            if (playlist != null) LogInfo($"ChangePlaylist: {playlist.Id}   {playlist.Name}   {playlist.Description}");
            else LogWarning("Warning: ChangePlaylist: playlist is null");

            var _songList = _dbContext.Songs.Where(s => s.Playlists.Contains(playlist) ).ToList();
            _songList = _songList.OrderBy(s => s.Title, StringComparer.OrdinalIgnoreCase).ToList();
            this.Dispatcher.Dispatch(() =>
            {
                TestSonglist.ItemsSource = _songList;
                TestSonglist.SelectedItem = _songList.FirstOrDefault();
            });
            
        }



        private void PlaySong(Song song)
        {

            if (song != null && song.PathName != null)
            {
                MediaSource _mediaSource = null;
                if (File.Exists(song.PathName)) _mediaSource = MediaSource.FromFile(song.PathName);
                else if (song.PathName.StartsWith("embed://")) _mediaSource = MediaSource.FromResource(song.PathName.Substring(8));
                LogInfo($"PlaySong: _mediaSource = {(_mediaSource == null ? "null" : _mediaSource.ToString())}");
                if (_mediaSource != null)
                {
                    this.Dispatcher.Dispatch(() =>
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



        private async Task SecondWindow(Application? app, ListView list)
        {
            // cjm - This works here but is almost certainly NOT the place it needs to go.
            // Second Window
            // https://devblogs.microsoft.com/dotnet/announcing-dotnet-maui-preview-11/
            //var secondWindow = new Window
            //{
            //    Page = new MySecondPage
            //    {
            //        // ...
            //    }
            //};
            //Application.Current.OpenWindow(secondWindow);
            // /Second Window



            // Let the main window open first.
            while (list.Height < 1) await Task.Delay(25);

            // and then let it populate, there should be at least one song?
            while (list.ItemsSource == null || list.ItemsSource.Cast<Song>().ToList().Count < 1) await Task.Delay(25);

            var secondWindow = new Window(new MyPage());
            secondWindow.Width = 1080;
            secondWindow.Height = 640;
            secondWindow.X = 25;
            secondWindow.Y = 25;
            secondWindow.Title = "AhLog Window";
            app.OpenWindow(secondWindow);
        }


    }
}
