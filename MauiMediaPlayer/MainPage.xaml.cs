using CommunityToolkit.Maui.Views;
using DataLibrary;
using static AngelHornetLibrary.AhLog;
using static CommonNet8.SearchForMusic;
using static CommonNet8.AllSongsPlaylist;
using static MauiMediaPlayer.ProgramLogic.StaticProgramLogic;
using CommonNet8;
using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;





namespace MauiMediaPlayer
{
    public partial class MainPage : ContentPage
    {

        private readonly PlaylistContext _dbContext;
        public static ConcurrentQueue<string> messageQueue { get; set; } = new ConcurrentQueue<string>();

        public MainPage(PlaylistContext dbcontext)
        //public MainPage()
        {
            // Change: Application.Current.MainPage.Dispatcher.Dispatch
            // to: this.Dispatcher.Dispatch

            InitializeComponent();
            _dbContext = dbcontext;

            if (Debugger.IsAttached)
            {
                var debugSpacer = new Label { Text = "Debug Toolbar Spacer ", HeightRequest = 48, HorizontalOptions = LayoutOptions.Center };
                MainGrid.Children.Add(debugSpacer);
                Grid.SetColumnSpan(debugSpacer, 2);
            }

            _ = DeliverMessageQueue(messageQueue, spinBox, messageBox);
            _ = SecondWindow(Application.Current, AngelHornetLogo);

            // Load Databases
            {
                LogDebug("Loading Saved Database");
                var _playlists = _dbContext.Playlists.ToList();
                _playlists = _playlists.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
                TestPlaylist.ItemsSource = _playlists;
                var _songList = _dbContext.Songs.ToList();
                _songList = _songList.OrderBy(s => s.Title, StringComparer.OrdinalIgnoreCase).ToList();
                TestSonglist.ItemsSource = _songList;
                LogMsg($"Loaded {_playlists.Count} Playlists, and {_songList.Count} Songs.");
                LogDebug("=== /Database Loading Complete =============================== ===");
            }

            // Search for More Music
            Task.Run(async () =>
            {
                var _progress = new ReportProgressToQueue(messageQueue);
                
                await SearchUserProfileMusic(_progress);
                await UpdateAllSongsPlaylist();

                LogDebug("Database Dispatch Start.");
                var _playlists = _dbContext.Playlists.ToList();
                _playlists = _playlists.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
                this.Dispatcher.Dispatch(() =>
                               TestPlaylist.ItemsSource = _playlists);

                var _songList = _dbContext.Songs.ToList();
                _songList = _songList.OrderBy(s => s.Title, StringComparer.OrdinalIgnoreCase).ToList();
                this.Dispatcher.Dispatch(() =>
                               TestSonglist.ItemsSource = _songList);
                await Task.Delay(1);
                LogDebug("=== /Database Dispatch Complete =============================== ===");
                await Task.Delay(5000);
                LogMsg("Startup Complete");
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
                LogMsg($"Playlist Selected: {_playList.Id}   {_playList.Name}   {_playList.Description}");
                ChangePlaylist(_playList);
            }
            else LogWarning("WARN[080] Playlist is null");
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

            LogMsg($"ChangeSong:{_selectedSongIndex}->{_newSongIndex}: {_title}");
            LogDebug($"   From[{_selectedSongIndex}]: {_selectedSong.Title}");
            LogDebug($"     To[{_newSongIndex}]: {_title}");

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
            if (playlist != null) LogMsg($"ChangePlaylist: {playlist.Id}   {playlist.Name}   {playlist.Description}");
            else LogWarning("WARN[124] playlist is null");

            var _songList = _dbContext.Songs.Where(s => s.Playlists.Contains(playlist)).ToList();
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
                if (song != null && song.Title != null) LogMsg($"PlaySong: {song.Title}");
                else LogWarning("WARN[147] Song is null");
                LogDebug($"PlaySong:{(_mediaSource == null ? "null" : _mediaSource.ToString())}");
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



        private async Task SecondWindow(Application? app, Image logo)
        {
            LogDebug("Mainpage Creating SecondWindow");
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
            while (logo.Height < 1) await Task.Delay(25);
            await Task.Delay(1000);

            var secondWindow = new Window(new MyPage());

            var _maximumWidth = (int)(DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density - Const.AppWidth - Const.AppDisplayBorder) / 2;
            secondWindow.Width = int.Max(Const.AppMinimumWidth, _maximumWidth);
            

            var _maximumHeight = (int)(DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density - Const.AppDisplayBorder);
            secondWindow.Height = int.Min(Const.AppHeight, _maximumHeight);

            secondWindow.X = 25;
            secondWindow.Y = 25;
            secondWindow.Title = "AhLog Window";
            app.OpenWindow(secondWindow);
            LogDebug("MainPage SecondWindow Creation Complete");
        }

        private async void SearchBar_SearchButtonPressed(object sender, EventArgs e)
        {
            var _searchBar = (SearchBar)sender;
            var _searchText = _searchBar.Text.ToLower();
            var _songList = new List<Song>();

            LogMsg($"Searching: '{_searchText}'");
            var _dbContext = new PlaylistContext();
            // {Title} {Artist} {Band} {Album} {Genre}
            _songList = await _dbContext.Songs.Where(s =>
                s.Title.ToLower().Contains(_searchText) ||
                s.Artist.ToLower().Contains(_searchText) ||
                s.Band.ToLower().Contains(_searchText) ||
                s.Album.ToLower().Contains(_searchText) ||
                s.Genre.ToLower().Contains(_searchText))
                .ToListAsync();

            if (_songList == null) _songList = new List<Song>();
            _songList = _songList.OrderBy(s => s.Title, StringComparer.OrdinalIgnoreCase).ToList();
            _dbContext.Dispose();

            LogMsg($"Found {_songList.Count} songs.");
            if (_songList.Count > 0)
                Application.Current.Dispatcher.Dispatch(() =>
                {
                    TestSonglist.ItemsSource = _songList;
                });
        }
    }
}
