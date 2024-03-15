using AhConfig;
using CommonNet8;
using DataLibrary;
using System.Collections.Concurrent;
using static AngelHornetLibrary.AhLog;
using static CommonNet8.AllSongsPlaylist;
using static CommonNet8.SearchForMusicFiles;
using static MauiMediaPlayer.ProgramLogic.StaticProgramLogic;




// *** WARNING *** Do NOT trust references numbers when it relates to XAML event handlers.
// await Task.Delay(1);  // This allows the GUI to render a frame, and has been inserted as needed where performance lagged, even in async contexts if it was needed.

namespace MauiMediaPlayer
{
    public partial class MainPage : ContentPage
    {

        private readonly PlaylistContext _dbContext;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly ISongRepository _songRepository;
        public static ConcurrentQueue<string> _messageQueue { get; set; } = new ConcurrentQueue<string>();
        private ConcurrentQueue<Song?> _cacheSongQueue = new ConcurrentQueue<Song>();
        private ConcurrentStack<Song> _playSongStack = new ConcurrentStack<Song>();
        private bool _repeatPlaylist = false;

        // Heinous Hack-Stockings!  This is a lot of trouble for one label!
        public static int FilePathFontSize = 1;
        public static int FilePathFrameHeight = 1;
        public static int FilePathFontMargin { get => -int.Max(FilePathFontSize / 2 - 2, 1); }
        public static string FilePathFontColor = "Transparent";
        public static int FilePathWindowWidth = 385;
        // /Hack-Stockings



        // Constructor Main Page
        public MainPage(PlaylistContext dbcontext, IPlaylistRepository playlistRepository, ISongRepository songRepository)
        //public MainPage()
        {
            // Change: Application.Current.MainPage.Dispatcher.Dispatch
            // to: this.Dispatcher.Dispatch

            InitializeComponent();
            SearchCount.Text = $"v{Const.InternalVersion}";
            _dbContext = dbcontext;
            _playlistRepository = playlistRepository;
            _songRepository = songRepository;

            _ = DeliverMessageQueue(_messageQueue, spinBox, messageBox);
            _ = SecondWindow(Application.Current, AngelHornetLogo);

            if (Const.UseSongCache)
            {
                _ = PlaySongTask();
                _ = CacheSongTask();
            }


            {
                // I went to a lot of trouble to create this, and now after input from others I'm going to make it permanent instead of a DEBUG_ONLY feature.
                FilePathFontSize = Const.SongPathFontSize;
                FilePathFrameHeight = Const.SongPathFrameHeight;
                FilePathFontColor = "Black";
            }

            // Load Databases
            {
                LogDebug("Loading Saved Database");
                var _playlists = _dbContext.Playlists.ToList();
                _playlists = _playlists.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
                TestPlaylist.ItemsSource = _playlists;
                var _songList = _dbContext.Songs.ToList();
                _songList = _songList.OrderBy(s => s.AlphaTitle, StringComparer.OrdinalIgnoreCase).ToList();
                _ = DispatchSonglist(_songList);
                if (_playlists.Count < 1) UpdateAllSongsPlaylist(_dbContext).Wait();  // cj this won't take effect till we re-load, but it's better than nothing.
                LogMsg($"Loaded {_playlists.Count} Playlists, and {_songList.Count} Songs.");
                LogDebug("=== /Database Loading Complete =============================== ===");
            }
            // /Load Databases

            // Load Advanced Search Help List
            Task.Run(async () =>
            {
                while (AdvancedSearchHelpList.Height < 1) await Task.Delay(25);
                this.Dispatcher.Dispatch(() =>
                {
                    List<string> list = new List<string>();
                    list = DataLibraryAdvancedSearch.ShortHelpText();
                    AdvancedSearchHelpList.ItemsSource = list;
                });
            });
            // / Advanced Search Help List

            // Search for More Music
            Task.Run(async () =>
            {
                var _progress = new ReportProgressToQueue(_messageQueue);

                await SearchUserProfileMusic(_dbContext, _progress);
                await UpdateAllSongsPlaylist(_dbContext);

                LogDebug("Database Dispatch Start");
                var _playlists = _dbContext.Playlists.ToList();
                _playlists = _playlists.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
                this.Dispatcher.Dispatch(() =>
                               TestPlaylist.ItemsSource = _playlists);

                var _songList = _dbContext.Songs.ToList();      // _dbContext does not allow OrdinalIgnoreCase
                _songList = _songList.OrderBy(s => s.AlphaTitle, StringComparer.OrdinalIgnoreCase).ToList();
                await DispatchSonglist(_songList);
                await Task.Delay(1);
                LogDebug("Database Dispatch Complete");
                //AddDoubleTapGesture(TestSonglist);
                TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.NumberOfTapsRequired = 2;
                tapGestureRecognizer.Tapped += (sender, args) =>
                {
                    var _sender = sender as ListView;
                    var _selected = _sender.SelectedItem;
                    if (_selected == null) return;
                    _sender.SelectedItem = null;
                    _sender.SelectedItem = _selected;
                    LogDebug("Double Tapped!");
                };
                //Dispatch Doubletap ... we have to hope they are loaded by then ... 
                LogDebug("Loading Double Tap Gesture(s) =============================== ===");
                while (TestPlaylist.Height < 1) await Task.Delay(25);               
                while (TestSonglist.Height < 1) await Task.Delay(25);
                this.Dispatcher.Dispatch( () =>
                {
                    TestPlaylist.GestureRecognizers.Add(tapGestureRecognizer);   
                    TestSonglist.GestureRecognizers.Add(tapGestureRecognizer);
                    LogDebug("=== Double Tap Gesture(s) Loaded");
                });

                await Task.Delay(1000);
                _ = this.Dispatcher.DispatchAsync(async () =>
                {
                    Enable_Gui(true);
                    await Task.Delay(1000);
                    LogMsg("*** Startup Complete ***");
                });
            });
            // /Search for More Music

        }  // /Constructor /Main Page

    }

}