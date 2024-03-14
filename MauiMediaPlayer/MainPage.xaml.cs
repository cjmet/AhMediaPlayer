using AhConfig;
using AngelHornetLibrary;
using CommonNet8;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using DataLibrary;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using static AngelHornetLibrary.AhLog;
using static CommonNet8.AllSongsPlaylist;
using static CommonNet8.SearchForMusicFiles;
using static MauiMediaPlayer.ProgramLogic.StaticProgramLogic;
using static DataLibrary.DataLibraryAdvancedSearch;
using Microsoft.EntityFrameworkCore;






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
            _dbContext = dbcontext;
            _playlistRepository = playlistRepository;
            _songRepository = songRepository;
            SearchCount.Text = $"v{Const.InternalVersion}";

            _ = DeliverMessageQueue(_messageQueue, spinBox, messageBox);
            _ = SecondWindow(Application.Current, AngelHornetLogo);

            if (Const.UseSongCache)
            {
                _ = PlaySongTask();
                _ = CacheSongTask();
            }

            //if (Debugger.IsAttached)
            //{
            //    // Hack to space stuff down below the debug toolbar.  I know we can collapse it, but it's still in the way even then.
            //    var debugSpacer = new Label { Text = "Debug Toolbar Spacer ", HeightRequest = 48, HorizontalOptions = LayoutOptions.Center };
            //    MainGrid.Children.Add(debugSpacer);
            //    Grid.SetColumnSpan(debugSpacer, 2);
            //}

            // I went to a lot of trouble to create this, and now after input from others I'm going to make it permanent instead of a DEBUG_ONLY feature.
            // Heinous Hack-Stockings!
            // cj - My God this is a Horrible Hack to get around broken Maui HeadTruncate Controls.  
            //       Worse it only updates when you run a new search to redraw the list.
            //       We could probably put an event in for on width change. 
            FilePathFontSize = Const.SongPathFontSize;
            FilePathFrameHeight = Const.SongPathFrameHeight;
            FilePathFontColor = "Black";
            // /Heinous Hack-Stockings

            // Load Databases
            {
                LogDebug("Loading Saved Database");
                var _playlists = _dbContext.Playlists.ToList();
                _playlists = _playlists.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
                TestPlaylist.ItemsSource = _playlists;
                var _songList = _dbContext.Songs.ToList();
                _songList = _songList.OrderBy(s => s.AlphaTitle, StringComparer.OrdinalIgnoreCase).ToList();
                DispatchSonglist(_songList);
                if (_playlists.Count < 1) UpdateAllSongsPlaylist(_dbContext).Wait();  // cj this won't take effect till we re-load, but it's better than nothing.
                LogMsg($"Loaded {_playlists.Count} Playlists, and {_songList.Count} Songs.");
                LogDebug("=== /Database Loading Complete =============================== ===");
            }

            // Load Advanced Search Help List
            Task.Run(async () =>
            {
                while (AdvancedSearchHelpList.Height < 1) await Task.Delay(25);
                Application.Current.Dispatcher.Dispatch(() =>
                {
                    List<string> list = new List<string>();
                    list = DataLibraryAdvancedSearch.ShortHelpText();
                    AdvancedSearchHelpList.ItemsSource = list;
                });
            });

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

                var _songList = _dbContext.Songs.ToList();
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
                this.Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(3), () =>
                {
                    while (TestPlaylist.Height < 1) Task.Delay(25);
                    TestPlaylist.GestureRecognizers.Add(tapGestureRecognizer);
                    while (TestSonglist.Height < 1) Task.Delay(25);
                    TestSonglist.GestureRecognizers.Add(tapGestureRecognizer);
                    LogDebug("=== Double Tap Gesture(s) Loaded");
                });
                LogDebug("Delay Loading Double Tap Gesture(s) =============================== ===");

                await Task.Delay(1000);
                _ = this.Dispatcher.DispatchAsync(async () =>
                {
                    Enable_Gui(true);
                    await Task.Delay(1000);
                    LogMsg("*** Startup Complete ***");     
                });
            });

        }  // /Constructor /Main Page



        private void TestPlaylist_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var _playList = (Playlist)e.SelectedItem;
            var _index = e.SelectedItemIndex;
            var _listView = (ListView)sender;
            _listView.Focus();

            SetEditBarState();
            if (_playList != null)
            {
                LogMsg($"Playlist Selected: {_playList.Id}   {_playList.Name}   {_playList.Description}");
                ChangePlaylist(_playList);
            }
            else LogMsg("Playlist is null");
        }

        private async void TestSonglist_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var _song = (Song)e.SelectedItem;
            var _index = e.SelectedItemIndex;
            var _listView = (ListView)sender;

            var _list = _listView.ItemsSource.Cast<Song>().ToList();
            if (Const.UseSongCache) await SongCache(_song, _list, _index);
            else PlaySong(_song, _song.PathName);
        }

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
                this.Dispatcher.Dispatch(() =>        // We definitely need to use dispatcher here.
                {
                    TestSonglist.SelectedItem = _newSong;
                    TestSonglist.ScrollTo(_newSong, ScrollToPosition.Start, true);
                });
            }
        }

        private async void ChangePlaylist(Playlist playlist)
        {
            if (playlist != null) LogMsg($"ChangePlaylist: {playlist.Id}   {playlist.Name}   {playlist.Description}");
            else LogWarning("WARN[124] playlist is null");

            var _songList = _dbContext.Songs.Where(s => s.Playlists.Contains(playlist)).ToList();
            _songList = _songList.OrderBy(s => s.AlphaTitle, StringComparer.OrdinalIgnoreCase).ToList();

            await DispatchSonglist(_songList);
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

        private async Task SongCache(Song _song, List<Song> _list, int _index)
        {
            // Cursory, Minimal, Check the Request
            // Is the SongCacheTask Running?
            // is the SongPlayTask Running?
            // clear the Song Queue
            // Signal the FileTransferTask
            // Reset contents of the Song Queue
            // Reset contents of the SongPlay Task

            // Quick Check
            LogDebug($"SongCache: {(_song != null ? _song.Title : "null")}");
            if (_song == null || _song.PathName == null)
            {
                LogDebug("Song is null");
                return;
            }




            // Clear the Queue
            _cacheSongQueue.Clear();
            _cacheSongQueue.Enqueue(null); // signal the queue to stop downloads

            // (Re)Fill the Queue, Queue it up fast, don't check the file systems, that just slows it all down.
            {
                for (int i = _index; i < _list.Count && i < _index + Const.CacheSize; i++)
                {
                    if (_list[i] == null) continue;
                    var _source = _list[i];
                    if (_source == null) continue;
                    if (_source == null || _source.PathName == null || _source.Title == null) continue;
                    LogTrace($" EnQueuing[{i - _index + 1}]: {_source.Title}");
                    _cacheSongQueue.Enqueue(_source);
                }
                LogDebug($" Queued: {_cacheSongQueue.Count} songs.");
            }

            // Signal the SongPlayTask
            _playSongStack.Push(_song);
        }

        private async Task<string> CopySongToCache(string _source, string _destination, long _fileSize, CancellationToken _token)
        {
            LogTrace($"Source: {_source}");
            LogTrace($"Destination: {_destination}");
            if (_source == null || _destination == null) return "";
            if (_token.IsCancellationRequested) return "";   /// smb over wan operations are slow, so check this often.

            if (!File.Exists(_source)) return "";
            if (_token.IsCancellationRequested) return "";

            // this should already be happening in a task, but it's lagging the GUI anyway
            // so lets wrap it in an 'async Task<long>' and see if that helps.
            // That helped, but It's still lagging the GUI.
            // Lets implement the isCached check in the caller to help this. We'll still have to call it once each, but should help.
            long _sourceSize;
            if (_fileSize > 0) _sourceSize = _fileSize;
            else
            {
                LogDebug($"  Getting File Size[293]: {_source}");
                _sourceSize = await Task.Run<long>(async () => { return new FileInfo(_source).Length; });
            }

            var _fileName = Path.GetFileNameWithoutExtension(_source);
            if (File.Exists(_destination) && new FileInfo(_destination).Length == _sourceSize)
            {
                LogDebug($"  Pre-Cached: {_fileName}");
                File.SetLastAccessTime(_destination, DateTime.Now);  /// Windows no longer updates this automatically.
                return _source;  // Pre-Cached
            }
            if (_token.IsCancellationRequested) return "";

            LogMsg($"  Caching: {_fileName}");
            try
            {
                // This can lag the UI up to 2 seconds when opening a file over the WAN.
                await using (FileStream sourceStream = File.OpenRead(_source))
                await using (FileStream destinationStream = File.Create(_destination))
                    await sourceStream.CopyToAsync(destinationStream, _token);

                // Lets try this to see if it's more responsive to the GUI. ... This is about the same, maybe a little more responsive, but less diagnostic.
                //await File.WriteAllBytesAsync(_destination, await File.ReadAllBytesAsync(_source, _token), _token);
            }
            catch (TaskCanceledException)
            {
                LogDebug($"  File Transfer Canceled");
                return "";
            }
            catch (Exception ex)
            {
                LogError($"CacheFile: {ex.Message}");
                return "";
            }
            LogTrace($"  Cached: {_fileName}");
            return _source;
        }

        private async Task CacheSongTask()
        {
            LogMsg($"SongCacheTask is Starting");
            LogDebug($"Song Cache Dir: {AppData.TmpDataPath}");
            var _song = new Song();
            var isCached = new List<string>();
            var _source = "";
            var _fileName = "";
            var _destination = "";
            var _cacheCleanDelay = 0;
            var _songTransferCTS = new CancellationTokenSource();
            var _copyTask = new Task<string>(() => { return null; });
            _copyTask.Start();

            while (true)
            {
                //if (_cacheSongQueue.TryDequeue(out _song))
                if (_cacheSongQueue.TryPeek(out _song))
                {
                    if (_song == null)
                    {
                        _cacheSongQueue.TryDequeue(out _song);  // eat the null/signal
                        while (!_cacheSongQueue.TryPeek(out _song) || _song == null) await Task.Delay(25);
                        if (_song != null && _song.PathName != null && _song.PathName == _source)
                        {
                            LogDebug($"  Primary Download Already in Progress: {_song.FileName}");
                            continue;  // we're already downloading the song we want next.
                        }
                        else if (_song != null && _song.PathName != null && isCached.Contains(_song.PathName))
                        {
                            LogDebug($"  Primary Pre-Cached: {_song.FileName}");
                            continue;  // we've already downloaded it.
                        }
                        // We are neither downloading, nor have we downloaded ... so cancel the current download and start the next one.
                        LogDebug($"Canceling File Transfer: {_fileName}");
                        _songTransferCTS.Cancel();
                        _songTransferCTS = new CancellationTokenSource();
                    }
                    else if (_copyTask.IsCanceled || _copyTask.IsCompleted || _copyTask.IsFaulted)
                    {
                        if (_copyTask.IsCompleted)
                        {
                            var _cached = _copyTask.Result;
                            if (_cached != null && _cached != "" && !isCached.Contains(_cached)) isCached.Add(_cached);
                        }
                        _cacheSongQueue.TryDequeue(out _song);
                        LogTrace($" DeQueuing[{_cacheSongQueue.Count + 1}]: {_song.Title}");

                        if (_song != null && _song.PathName != null && isCached.Contains(_song.PathName))
                        {
                            LogDebug($"  Secondary PreCached: {_song.FileName}");
                            continue;  // we've already downloaded it.
                        }

                        _songTransferCTS = new CancellationTokenSource();
                        var _token = _songTransferCTS.Token;
                        _source = _song.PathName;
                        _fileName = _song.FileName;
                        _destination = Path.Combine(AppData.TmpDataPath, Path.GetFileName(_song.PathName));
                        _copyTask = CopySongToCache(_source, _destination, _song.FileSize, _token);
                        if (_song != null) await Task.Delay(Const.ClockTick);  // Need the pause here for responsiveness.
                    }
                    else
                    {
                        LogTrace($"  Copying: {_fileName}");
                        await Task.Delay(Const.ClockTick);
                    }
                }
                else
                {
                    //LogTrace(" SongCacheTask is Waiting");
                    if (_cacheCleanDelay-- <= 0)
                    {
                        LogTrace("Cleaning Cache");
                        var _files = new DirectoryInfo(AppData.TmpDataPath)
                       .EnumerateFiles()
                       .Where(f => f.Extension != ".db" && f.Extension != ".db-shm" && f.Extension != ".db-wal")
                       .OrderByDescending(f => f.LastAccessTime);
                        var _fileCount = 0;
                        foreach (var _file in _files.Skip(Const.CacheSizeMax))
                        {
                            LogTrace($"Un-Caching: [{_file.LastAccessTime}] {_file.Name}");
                            _file.Delete();
                            _fileCount++;
                        }
                        LogTrace($"Cleaning Cache: {_fileCount} files removed.");
                        _cacheCleanDelay = Const.CacheCleanInterval;
                    }
                    await Task.Delay(Const.ClockTick);
                }
            }
        }

        private async Task PlaySongTask()
        {
            LogMsg("SongPlayTask is Starting");
            var _song = new Song();
            var _source = "";
            long _sourceSize = 0;
            var _destination = "";
            var spinner = 0;
            while (true)
            {
                if (_playSongStack.TryPop(out Song _tmp))
                {
                    string poppedTitle;
                    if (_tmp != null && _tmp.Title != null) poppedTitle = _tmp.Title;
                    else poppedTitle = "null";
                    LogDebug($"  Popping[{_playSongStack.Count + 1}]: {poppedTitle}");
                    if (_tmp == null) continue;
                    _song = _tmp;
                    _source = _song.PathName;
                    if (_song.FileSize > 0) _sourceSize = _song.FileSize;
                    else _sourceSize = 0;
                    _destination = Path.Combine(AppData.TmpDataPath, Path.GetFileName(_song.PathName));
                    _playSongStack.Clear();
                }
                else if (File.Exists(_destination) && new FileInfo(_destination).Length > 8)
                {
                    var _destinationSize = new FileInfo(_destination).Length;

                    if (_sourceSize == 0)
                    {
                        LogDebug($"  Getting File Size[455]: {_source}");
                        _sourceSize = await Task.Run<long>(async () => { return new FileInfo(_source).Length; });  // lets only do this once.
                    }
                    if (_sourceSize == _destinationSize)
                    {
                        PlaySong(_song, _destination);
                        _source = _destination = "";
                    }
                    else
                    {
                        double _percent = (double)_destinationSize / _sourceSize;
                        var denom = Const.INF / Const.ClockTick;
                        if (spinner++ % denom == 0)
                            LogMsg($"  Loading[{_percent:P0}]: {Path.GetFileNameWithoutExtension(_source)}");
                        await Task.Delay(Const.ClockTick);
                    }
                }
                else
                {
                    //LogTrace("  SongPlayTask is Waiting"); 
                    await Task.Delay(Const.ClockTick);
                }
            }

        }

        private async Task SecondWindow(Application? app, Image logo)
        {
            LogDebug("Mainpage Creating SecondWindow");
            // cj - This works here but is almost certainly NOT the place it needs to go.
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

            var _maximumWidth = (int)(DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density - Const.AppWidth - Const.AppDisplayBorder);
            secondWindow.Width = int.Max(Const.AppMinimumWidth, _maximumWidth);
            secondWindow.Width = int.Min(Const.AppMaximumWidth, (int)secondWindow.Width);


            var _maximumHeight = (int)(DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density - Const.AppDisplayBorder);
            secondWindow.Height = int.Min(Const.AppHeight, _maximumHeight);

            secondWindow.X = 25;
            secondWindow.Y = 25;
            secondWindow.Title = "AhLog Window";
            app.OpenWindow(secondWindow);
            LogDebug("MainPage SecondWindow Creation Complete");
        }

        private async void Shuffle_Clicked(object sender, EventArgs e)
        {
            LogMsg("Shuffle");
            var button = (Button)sender;
            var _songList = TestSonglist.ItemsSource.Cast<Song>().ToArray();
            new Random().Shuffle(_songList);
            var _list = _songList.ToList();
            await DispatchSonglist(_list);

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
                Application.Current.Dispatcher.Dispatch(() => TestSonglist.SelectedItem = _newSong);
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
                Application.Current.Dispatcher.Dispatch(() => TestSonglist.SelectedItem = _newSong);
        }

        private void RepeatList_Clicked(object sender, EventArgs e)
        {
            LogMsg($"Repeat Playlist: {_repeatPlaylist}");
            _repeatPlaylist = !_repeatPlaylist;
            Application.Current.Dispatcher.Dispatch(() =>
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
                Application.Current.Dispatcher.Dispatch(() => TestSonglist.SelectedItem = _newSong);
        }

        private void LastTrack_Clicked(object sender, EventArgs e)
        {
            LogMsg("Last Track");
            var button = (Button)sender;
            var _songList = TestSonglist.ItemsSource.Cast<Song>().ToList();
            var _newSong = _songList.ElementAtOrDefault(_songList.Count - 1);
            if (_newSong != null)
                Application.Current.Dispatcher.Dispatch(() => TestSonglist.SelectedItem = _newSong);
        }

        private void RandomPersistentLogo(string data, int found)
        {
            LogTrace($"RandomPersistentLogo: {data}"); // cj 
            data = data.ToLower().Trim();
            var num = data.GetHashCode();
            var images = new string[]
            {   // cj - I can't seem to find a way to read these, so I'm just going to hard code them.
                "angel_hornet_logo_cropped.png", "azrael_lc.png",
                "bad_grim_lc.png", "baxters_lc.png", "big_moon_lc.png",
                "brie_lc.png", "bubby_lc.png", "chex_lc.png", "damnit_gizmo_lc.png",
                "django_lc.png", "dot_lc.png", "fox_lc.png", "herc_lc.png",
                "howard_lc.png", "huggy_lc.png", "kibbs_lc.png", "kissy_lc.png",
                "lucy_lc.png", "luna_lc.png", "nino_lc.png", "pie_lc.png",
                "possum_lc.png", "rey_lc.png", "spicey_lc.png", "stella_lc.png",
                "tiger_lc.png"
            };

            var index = (uint)num % images.Length;
            if (data == "") index = 0;
            LogTrace($"RandomPersistentLogo: {index} / {images.Length}");

            var image = images[index];
            var Name = Path.GetFileNameWithoutExtension(image);
            if (Name == "angel_hornet_logo_cropped") Name = "Angel Hornet";
            var Names = Name.Split('_');
            Name = "";
            foreach (var n in Names)
            {
                if (n.Length >= 1) Name += n.Substring(0, 1).ToUpper() + n.Substring(1) + " ";
            }
            Name = Name.Trim();
            Name = Name.Replace(" Lc", "");                 // Curse you GitHub for making me rename the files so you'd respect lower case!
            LogMsg($"{Name} found you {found} Songs!");

            Application.Current.Dispatcher.Dispatch(() =>
            {
                AngelHornetLogo.Source = ImageSource.FromFile(images[index]);
            });
        }

        private async void SearchDirectories_Clicked(object sender, EventArgs e)
        {
            var folder = Environment.SpecialFolder.UserProfile;
            var _tmpPath = Environment.GetFolderPath(folder);
            _tmpPath = Path.Join(_tmpPath, "Music");
            var _searchDir = await FolderPicker.PickAsync(_tmpPath);  // cj 
            if (_searchDir == null || _searchDir.Folder == null) return;
            var _path = _searchDir.Folder.Path;
            LogMsg($"SearchDirectories: {_path}");
            if (_path != "")
            {
                var _progress = new ReportProgressToQueue(_messageQueue);
                _ = SearchUserProfileMusic(_dbContext, _progress, _path);
            }
        }

        private void MenuBox_Clicked(object sender, EventArgs e)
        {
            if (AdvandedSearchFrame.IsVisible) Application.Current.Dispatcher.Dispatch(() =>
            {
                AdvandedSearchFrame.IsVisible = false;
                MenuBox.BackgroundColor = Color.Parse("Transparent");
                StandardSearchBar.IsEnabled = true;
            });
            else Application.Current.Dispatcher.Dispatch(() =>
            {
                SetEditBarState();
                AdvandedSearchFrame.IsVisible = true;
                MenuBox.BackgroundColor = Color.Parse("LightBlue");
                Searchby.SelectedIndex = 0;
                SearchAction.SelectedIndex = 0;
                StandardSearchBar.IsEnabled = false;
            });
        }
        private void SetEditBarState()
        {
            Playlist? _playlist = (Playlist)TestPlaylist.SelectedItem;
            if (_playlist == null || _playlist.Id == 1) Application.Current.Dispatcher.Dispatch(() =>
            {
                AddSongsGui.Opacity = 0.25;
                AddSongsGui.IsEnabled = false;
                RemoveSongsGui.Opacity = 0.25;
                RemoveSongsGui.IsEnabled = false;
                DeletePlaylistGui.Opacity = 0.25;
                DeletePlaylistGui.IsEnabled = false;
                EditPlaylistGui.Opacity = 0.25;
                EditPlaylistGui.IsEnabled = false;
            });
            else if (_playlist.Id > 1) Application.Current.Dispatcher.Dispatch(() =>
            {
                AddSongsGui.Opacity = 1;
                AddSongsGui.IsEnabled = true;
                RemoveSongsGui.Opacity = 1;
                RemoveSongsGui.IsEnabled = true;
                DeletePlaylistGui.Opacity = 1;
                DeletePlaylistGui.IsEnabled = true;
                EditPlaylistGui.Opacity = 1;
                EditPlaylistGui.IsEnabled = true;
            });
        }
        private async void SearchBar_SearchButtonPressed(object sender, EventArgs e) => AdvancedSearchBar_SearchButtonPressed(sender, e);
        private async void AdvancedSearchBar_SearchButtonPressed(object sender, EventArgs e)
        {
            var _searchBar = (SearchBar)sender;
            var _searchText = _searchBar.Text;
            if (_searchText == null) _searchText = "";


            string? _by;
            if (Searchby.SelectedItem == null) _by = "Any";
            else _by = Searchby.SelectedItem.ToString().ToLower();

            string? _action;
            if (SearchAction.SelectedItem == null) _action = "Search";
            else _action = SearchAction.SelectedItem.ToString().ToLower();

            List<Song> _currentSet = new List<Song>();
            if (TestSonglist != null && TestSonglist.ItemsSource != null) _currentSet = TestSonglist.ItemsSource.Cast<Song>().ToList();

            // Intercept with AdvancedSearchParse 
            List<Song>? _advancedResult;
            List<Song> _songList = _currentSet;
            string? _searchBy = "Any";
            string? _searchAction = "SEARCH";

            // *** Advanced Search Parse ***
            LogTrace("===");
            if (AhLog._LoggingLevel.MinimumLevel < Serilog.Events.LogEventLevel.Debug)
                LogTrace($"Advanced Search by: '{_by}'   Action: '{_action}'   SearchText: '{_searchText}'");
            else
                LogMsg($"Search: \"{_searchText}\"");


            (_advancedResult, _searchBy, _searchAction) = AdvancedSearch(_currentSet, _searchText, _by, _action, _dbContext);
            if (_advancedResult != null)
            {
                _songList = _advancedResult.ToList();
                if (_searchBy != null) _by = _searchBy;
                if (_searchAction != null) _action = _searchAction;
            }

            if (_searchAction != "IS" && _advancedResult != null) RandomPersistentLogo(_searchText, _songList.Count);

            if (_songList.Count > 0)
            {
                await DispatchSonglist(_songList);
            }

            Application.Current.Dispatcher.Dispatch(() =>
            {
                if (_songList.Count > 0) SearchCount.Text = $"{_songList.Count:n0}";
                Searchby.SelectedItem = _searchBy;
                SearchAction.SelectedItem = _searchAction;
                var Placeholder = "Title, Artist, Band, Album, Genre, Path";
                if (_by != null && _action != null && ( _by.ToLower() != "any"  || _action.ToUpper() != "SEARCH")) Placeholder = $"SearchAction: {_searchAction}     -     SearchBy: {_searchBy}";
                _searchBar.Placeholder = Placeholder;
            });
        }

        private void FilePathDebug_SizeChanged(object sender, EventArgs e)
        {
            var _label = (Label)sender;

            // cj - This is working, but there HAS to be another better way!
            //_label.SetBinding(Label.TextProperty, new Binding("PathName", source: _label.BindingContext, converter: new HeadTruncateConverter()));  // This might be correct Syntax?!?  Co-Pilot suggested it.
            FilePathWindowWidth = int.Max((int)TestSonglist.Width, Const.AppMinimumWidth);
            var tmp = _label.BindingContext;
            _label.BindingContext = null;
            _label.BindingContext = tmp;
        }

        private Boolean Gui_SaveAs = true;        // SaveAS vs Save vs Edit
        private void SaveAsPlaylistGui_Clicked(object sender, EventArgs e)
        {
            Gui_SaveAs = true;
            var _sender = (Button)sender;

            if (SaveAsPlaylistFrame.IsVisible) Application.Current.Dispatcher.Dispatch(() =>
            {
                SaveAsPlaylistFrame.IsVisible = false;
                SaveAsPlaylistName.Text = "";
                SaveAsPlaylistDesc.Text = "";
            });

            else Application.Current.Dispatcher.Dispatch(() =>
            {
                SaveAsPlaylistFrame.IsVisible = true;
                SaveAsPlaylistName.Text = "";
                SaveAsPlaylistDesc.Text = "";
            });
            return;

        }


        private async void DoSavePlaylistFrame_Clicked(object sender, EventArgs e)
        {
            Enable_Gui(false);
            await Task.Delay(1);

            var _sender = (Button)sender;

            var _name = SaveAsPlaylistName.Text;
            var _desc = SaveAsPlaylistDesc.Text;

            if (_name == null || _name == "")
            {
                var _lists = await _dbContext.Playlists.ToListAsync();
                var _last = _lists?.LastOrDefault();
                if (_last != null && _last.Id != null && _last.Id > 0)
                {
                    if (Gui_SaveAs) _name = $"Playlist {_last.Id + 1}";
                    else
                    {
                        var _playlist = (Playlist)TestPlaylist.SelectedItem;
                        if (_playlist == null) { Enable_Gui(true); return; }
                        _name = $"Playlist {_playlist.Id}";
                    }
                }
                else
                {
                    LogError("Playlists are null.");
                    _name = null;
                }
            }
            if (_desc == null) _desc = "";



            if (_name != null)
            {
                Playlist _playlist = new Playlist();
                if (Gui_SaveAs)
                {

                    _playlist = new Playlist { Name = _name, Description = _desc, Songs = new List<Song>() };
                    _dbContext.Playlists.Add(_playlist);
                    List<Song>? _songList = new List<Song>();
                    await Task.Run(() => _songList = TestSonglist.ItemsSource.Cast<Song>().ToList());
                    if (_songList == null) _songList = new List<Song>();
                    int i = 0;
                    foreach (Song _song in _songList)
                        if (_song != null)
                        {
                            var _addSong = _dbContext.Songs.Find(_song.Id);             // EF Core Tracking I Hate You.
                            if (_addSong != null) _playlist.Songs.Add(_addSong);
                            if (i++ % 400 == 0) await Task.Delay(1);                    // roughly 25 fps on my machine.
                            if (i % 10000 == 0) LogMsg($"Adding Songs: {i}");
                        }
                    _dbContext.SaveChanges();
                }
                else
                {
                    _playlist = (Playlist)TestPlaylist.SelectedItem;
                    if (_playlist == null) { Enable_Gui(true); return; }
                    _playlist.Name = _name;
                    _playlist.Description = _desc;
                    _dbContext.SaveChanges();
                }

                LogMsg($"Playlist Saved: [{_playlist.Id}] {_playlist.Name} - {_playlist.Description}");
                var _playlists = _dbContext.Playlists.ToList();
                _playlists = _playlists.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
                Application.Current.Dispatcher.Dispatch(async () =>
                {
                    TestPlaylist.ItemsSource = _playlists;
                    await Task.Delay(1);
                    TestPlaylist.SelectedItem = _playlist;
                    await Task.Delay(1);
                    TestPlaylist.ScrollTo(_playlist, ScrollToPosition.Start, true);
                });
            }

            Application.Current.Dispatcher.Dispatch(() =>
            {
                SaveAsPlaylistFrame.IsVisible = false;
                SaveAsPlaylistName.Text = "";
                SaveAsPlaylistDesc.Text = "";
            });

            Enable_Gui(true);
        }

        private async void DeletePlaylistImproved(object sender, EventArgs e)
        {
            // Removing the songs first, with a smart selective list, makes the delete playlist 100x faster.
            // So we're reusing what we learned in Holy Swiss Cheese, then doing the playlist delete.
            LogMsg("Intercepting DeletePlaylistGui_Clicked");
            var _songs = (List<vSong>)TestSonglist.ItemsSource;
            if (_songs == null) return;
            
            Enable_Gui(false);
            await AddRemoveSongList(sender, e, _songs, false);
            
            var _appPlaylist = (Playlist)TestPlaylist.SelectedItem;
            var _playlistID = _appPlaylist.Id;

            var _playlists = await _dbContext.Playlists.ToListAsync();
            _playlists = _playlists.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
            var _playListIndex = _playlists.IndexOf(_appPlaylist);

            var _dbPlaylist = await _dbContext.Playlists.Where(p => p.Id == _playlistID).FirstOrDefaultAsync();
            if (_dbPlaylist == null || _playlistID <= 1) { Enable_Gui(true); return; }

            await Task.Run(async () =>
            {
                LogDebug($"Removing[967] ... ");
                _dbContext.Playlists.Remove(_dbPlaylist);
                LogDebug($"Removed[968] ... ");
                await Task.Delay(1);
                LogDebug($"Saving[973] ... ");
                _dbContext.SaveChanges();
                LogDebug($"Saved[975] ... ");
            });

            TestPlaylist.SelectedItem = null;
            _playlists = await _dbContext.Playlists.ToListAsync();
            _playlists = _playlists.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
            var _index = _playListIndex - 1;
            _index = _index < 0 ? 0 : _index;   
            _appPlaylist = _playlists.ElementAtOrDefault(_index);
            LogDebug($"Dispatching[976] ... ");
            await Application.Current.Dispatcher.DispatchAsync(async () =>
            {
                TestPlaylist.ItemsSource = _playlists;
                await Task.Delay(1);
                if (_appPlaylist != null)
                {
                    TestPlaylist.ScrollTo(_appPlaylist, ScrollToPosition.Start, true);
                    TestPlaylist.SelectedItem = _appPlaylist;
                }
            });

            Enable_Gui(true);

            return;
        }
        private async void DeletePlaylistGui_Clicked(object sender, EventArgs e)
        {

            var _sender = (Button)sender;
            var _playlist = (Playlist)TestPlaylist.SelectedItem;
            if (_playlist == null || _playlist.Id == 1) return;

            bool answer = await DisplayAlert("Delete Playlist", _playlist.Name, "Delete", "Cancel");

            // cjm 
            // Interecept this and lets see if we can make it better.
            if (answer) DeletePlaylistImproved(sender, e);
            return;


            //if (answer)
            //{
            //    Enable_Gui(false);


            //    var spinner = 0;
            //    var spinchar = new char[] { '|', '/', '-', '\\' };
            //    var cts = new CancellationTokenSource();
            //    var ct = cts.Token;


            //    Task spin = new Task(async () =>
            //    {
            //        var id = _playlist.Id;
            //        var name = _playlist.Name;
            //        while (!ct.IsCancellationRequested)
            //        {
            //            LogMsg($"Deleting Playlist[{id}]: {(spinner++ == 0 ? name : spinchar[spinner % 4])}");
            //            await Task.Delay(3000);
            //        }
            //    }, ct, TaskCreationOptions.LongRunning);
            //    spin.Start();

            //    LogDebug($"Deleting Playlist[990]: [{_playlist.Id}] {_playlist.Name}");
            //    // WARNING:  You can't do StringComparer in dbContext.  I knew that, but forgot.
            //    var _playlists = await _dbContext.Playlists.ToListAsync();
            //    _playlists = _playlists.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
            //    var _index = _playlists.IndexOf(_playlist);
            //    if (--_index < 0) _index = 0;
            //    await Task.Run(() =>
            //    {
            //        LogDebug($"Removing[967] ... ");
            //        _dbContext.Playlists.Remove(_playlist);
            //        LogDebug($"Removed[968] ... ");
            //        LogDebug($"Saving[973] ... ");
            //        _dbContext.SaveChanges();
            //        LogDebug($"Saved[975] ... ");
            //    });
            //    TestPlaylist.SelectedItem = null;
            //    _playlists = await _dbContext.Playlists.ToListAsync();    
            //    _playlists = _playlists.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
            //    _playlist = _playlists.ElementAtOrDefault(_index);
            //    LogDebug($"Dispatching[976] ... ");
            //    await Application.Current.Dispatcher.DispatchAsync(async () =>
            //    {
            //        TestPlaylist.ItemsSource = _playlists;
            //        await Task.Delay(1);
            //        if (_playlist != null)
            //        {
            //            TestPlaylist.ScrollTo(_playlist, ScrollToPosition.Start, true);
            //            TestPlaylist.SelectedItem = _playlist;
            //        }
            //    });

            //    cts.Cancel();
            //    Enable_Gui(true);
            //}



        }

        private void Enable_Gui(bool _enable)
        {
            LogMsg($"* {(_enable ? "En" : "Dis")}abling Controls");
            var _opacity = _enable ? 1 : 0.25;
            TestPlaylist.IsEnabled = _enable;
            TestPlaylist.Opacity = _opacity;
            EditFrame.IsEnabled = _enable;
            EditFrame.Opacity = _opacity;
            StandardSearchBar.IsEnabled = _enable;
            StandardSearchBar.Opacity = _opacity;
            AdvancedSearchGrid.IsEnabled = _enable;
            AdvancedSearchGrid.Opacity = _opacity;
            MenuBox.IsEnabled = _enable;
            MenuBox.Opacity = _opacity;
        }
        private async Task AddRemoveSongList(object sender, EventArgs e, List<vSong>? _songlist, Boolean _addSongs)
        {
            // cjm - Holy Swiss Cheese!  
            // Adding a song that's already added causes a MASSIVE slowdown and even causes the SaveChangesAsync to hard lock sometimes.
            // It might be better to use SaveChanges() no Async.
            // However, making a 'smart' list of changes only, avoids the 'setting already set' problem which in turn avoids the massive slowdown and hard lock.
            // Just 30 songs 'setting already set' was enough to cause a hard lock.
            // ... now to test more extensively.

            LogMsg("Intercepting Add/Remove Song");
            var _sender = (Button)sender;
            var _playlist = (Playlist)TestPlaylist.SelectedItem;
            var _playlistId = _playlist.Id;
            //var _songlist = (List<vSong>)TestSonglist.ItemsSource; // Pass this in instead, then I can use this method for other things as well.
            var _songlistIds = _songlist.Select(s => s.Id).ToList();
            if (_playlist == null || _playlist.Id <= 1) return;
            if (_songlist == null) return;

            await Task.Delay(1);
            if (_addSongs)
            {
                var _existingIds = await _dbContext.Songs.Where(s => s.Playlists.Any(p => p.Id == _playlistId)).Select(s => s.Id).ToListAsync();
                var _newSongIds = _songlistIds.Except(_existingIds).ToList();
                LogMsg($"Adding {_newSongIds.Count} Songs to Playlist (DB)[{_playlistId}]");
                // cjm - check for null playlists.  Use "?" notation!
                await _dbContext.Songs.Where(s => _newSongIds.Contains(s.Id)).Include(s => s.Playlists).ForEachAsync(s =>{ s.Playlists?.Add(_playlist); });  
                await Task.Delay(1);

                // Stars are just a visual aid, we don't have to actually set them in the database, just the View.
                LogMsg($"Setting Stars {_newSongIds.Count} Songs to Playlist (local)[{_playlistId}]");
                foreach (var song in _songlist.Where(s => _newSongIds.Contains(s.Id))) song.Star = true;

                LogMsg($"Saving ... ");
                var results = await _dbContext.SaveChangesAsync();
                if (results < _newSongIds.Count ) LogError($"ERROR: Saved [{results}] of [{_newSongIds.Count}] ");
                LogMsg($"Saved [{results}] ... ");
            }
            else
            {
                var _existingIds = await _dbContext.Songs.Where(s => s.Playlists.Any(p => p.Id == _playlistId)).Select(s => s.Id).ToListAsync();
                var _removeSongIds = _songlistIds.Intersect(_existingIds).ToList();
                LogMsg($"Removing {_removeSongIds.Count} Songs from Playlist (DB)[{_playlistId}]");
                // cjm - check for null playlists.  Use "?" notation!
                await _dbContext.Songs.Where(s => _removeSongIds.Contains(s.Id)).Include(s => s.Playlists).ForEachAsync(s => { s.Playlists?.Remove(_playlist); }); 
                await Task.Delay(1);

                // Stars are just a visual aid, we don't have to actually set them in the database, just the View.
                LogMsg($"Setting Stars {_removeSongIds.Count} Songs from Playlist (local)[{_playlistId}]");
                foreach (var song in _songlist.Where(s => _removeSongIds.Contains(s.Id))) song.Star = false;

                LogMsg($"Saving ... ");
                var results = await _dbContext.SaveChangesAsync();
                if (results < _removeSongIds.Count) LogError($"ERROR: Saved [{results}] of [{_removeSongIds.Count}] ");
                LogMsg($"Saved [{results}] ... ");
            }
        }
        private async void AddSongsGui_Clicked(object sender, EventArgs e)    // cjm 
        {
            // Intercept and lets see if we can do this more efficiently.
            var _songs = (List<vSong>)TestSonglist.ItemsSource;
            if (_songs == null) return;
            Enable_Gui(false);
            await AddRemoveSongList(sender, e, _songs, true);
            Enable_Gui(true);
            return;


            //LogMsg("Checking Playlist");
            //var _playlistCheck = (Playlist)TestPlaylist.SelectedItem;
            //if (_playlistCheck == null || _playlistCheck.Id <= 1) return;
            //Enable_Gui(false);
            //LogMsg($"Reading Songs ... ");
            //var _sender = (Button)sender;
            //var _songs = (List<vSong>)TestSonglist.ItemsSource;
            //if (_songs != null)
            //{
            //    LogMsg($"Adding {_songs.Count} Songs");
            //    int i = 0;
            //    foreach (var _song in _songs)
            //    {
            //        if (_song != null) AddRemoveSong(_song, true);
            //        if (++i % 30 == 0) LogMsg($"Adding Songs: {i}");
            //        await Task.Delay(1);
            //    }
            //}
            //Enable_Gui(true);
        }
        private async void RemoveSongsGui_Clicked(object sender, EventArgs e)
        {
            // Intercept and lets see if we can do this more efficiently.
            var _songs = (List<vSong>)TestSonglist.ItemsSource;
            if (_songs == null) return;
            Enable_Gui(false);
            await AddRemoveSongList(sender, e, _songs, false);
            Enable_Gui(true);
            return;

            //LogMsg("Checking Playlist");
            //var _playlistCheck = (Playlist)TestPlaylist.SelectedItem;
            //if (_playlistCheck == null || _playlistCheck.Id <= 1) return;
            //Enable_Gui(false);
            //LogMsg($"Reading Songs ... ");
            //var _sender = (Button)sender;
            //var _songs = (List<vSong>)TestSonglist.ItemsSource;
            //if (_songs != null)
            //{
            //    LogMsg($"Remove {_songs.Count} Songs");
            //    int i = 0;
            //    foreach (var _song in _songs)
            //    {
            //        if (_song != null) await AddRemoveSong(_song, false);
            //        if (++i % 30 == 0) LogMsg($"Removing Songs: {i}");
            //        await Task.Delay(1);
            //    }
            //    Enable_Gui(true);
            //}
        }
        private async Task AddRemoveSong(vSong _song, bool _add)
        {
            throw new Exception("This is not used anymore.");
            //if (_song == null) return;
            //var _playlist = (Playlist)TestPlaylist.SelectedItem;
            //if (_playlist == null) return;
            //if (_playlist.Id == 1) return;

            //var _dbSong = await _dbContext.Songs.Include(s => s.Playlists).Where(s => s.Id == _song.Id).FirstOrDefaultAsync();
            //if (_dbSong == null) return;
            //var _dbPlaylist = await _dbContext.Playlists.Where(p => p.Id == _playlist.Id).FirstOrDefaultAsync();
            //if (_add)
            //{
            //    _song.Star = true;
            //    _dbSong.Playlists.Add(_playlist);
            //    if (_playlist.Songs != null) _playlist.Songs.Add(_dbSong);
            //}
            //else
            //{
            //    _song.Star = false;
            //    _dbSong.Playlists.Remove(_playlist);
            //    if (_playlist.Songs != null) _playlist.Songs.Remove(_dbSong);
            //}

            //var results = _dbContext.SaveChanges();
            //LogTrace($"Id[{_playlist.Id}]: {_playlist.Name}   [{(_add ? "Adding" : "Removing")}]: {_song.Title}   Results: {results}");
        }

        private void OnStar_Clicked(object sender, EventArgs e)
        {
            var _sender = (Button)sender;
            var _song = (vSong)_sender.BindingContext;
            if (_song == null) return;
            var _playlist = (Playlist)TestPlaylist.SelectedItem;
            if (_playlist == null) return;
            if (_playlist.Id == null) return;
            if (_playlist.Id <= 1) return;

            LogDebug($"Star Song[{_song.Star}]: {_song.Title}");
            _song.Star = !_song.Star;

            var _dbSong = _dbContext.Songs.Include(s => s.Playlists).Where(s => s.Id == _song.Id).FirstOrDefault();
            var _dbPlaylist = _dbContext.Playlists.Where(p => p.Id == _playlist.Id).FirstOrDefault();
            if (_song.Star)
            {
                _dbSong.Playlists.Add(_playlist);
                if (_playlist.Songs != null) _playlist.Songs.Add(_dbSong);
            }
            else
            {
                _dbSong.Playlists.Remove(_playlist);
                if (_playlist.Songs != null) _playlist.Songs.Remove(_dbSong);
            }

            var results = _dbContext.SaveChanges();
            LogMsg($"Id[{_playlist.Id}]: {_playlist.Name}   [{(_song.Star ? "Adding" : "Removing")}]: {_song.Title}   Results: {results}");
        }

        private void EditPlaylistGui_Clicked(object sender, EventArgs e)
        {
            Gui_SaveAs = false;
            var _sender = (Button)sender;
            var _playlist = (Playlist)TestPlaylist.SelectedItem;
            if (_playlist == null || _playlist.Id == 1) return;
            var _name = _playlist.Name;
            var _desc = _playlist.Description;
            if (_name == null) _name = "";
            if (_desc == null) _desc = "";

            if (SaveAsPlaylistFrame.IsVisible) Application.Current.Dispatcher.Dispatch(() =>
            {
                SaveAsPlaylistFrame.IsVisible = false;
                SaveAsPlaylistName.Text = "";
                SaveAsPlaylistDesc.Text = "";
            });

            else Application.Current.Dispatcher.Dispatch(() =>
            {
                SaveAsPlaylistFrame.IsVisible = true;
                SaveAsPlaylistName.Text = _name;
                SaveAsPlaylistDesc.Text = _desc;
            });
            return;

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