using CommonNet8;
using CommunityToolkit.Maui.Views;
using DataLibrary;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using static AngelHornetLibrary.AhLog;
using static CommonNet8.AllSongsPlaylist;
using static CommonNet8.SearchForMusic;
using static MauiMediaPlayer.ProgramLogic.StaticProgramLogic;
using AhConfig;



namespace MauiMediaPlayer
{
    public partial class MainPage : ContentPage
    {

        private readonly PlaylistContext _dbContext;
        public static ConcurrentQueue<string> _messageQueue { get; set; } = new ConcurrentQueue<string>();
        private ConcurrentQueue<Song?> _cacheSongQueue = new ConcurrentQueue<Song>();
        private ConcurrentStack<Song> _playSongStack = new ConcurrentStack<Song>();
        private bool _repeatPlaylist = false;


        // Heinous Hack-Stockings!  This is a lot of trouble for one label!
        public static int FilePathFontSize = 1;
        public static int FilePathFontMargin { get => -int.Max(FilePathFontSize / 2 - 1, 1); }
        public static string FilePathFontColor = "Transparent";
        public static int FilePathWindowWidth = 385;
        // /Hack-Stockings



        public MainPage(PlaylistContext dbcontext)
        //public MainPage()
        {
            // Change: Application.Current.MainPage.Dispatcher.Dispatch
            // to: this.Dispatcher.Dispatch

            InitializeComponent();
            _dbContext = dbcontext;

            if (Debugger.IsAttached)
            {
                // Hack to space stuff down below the debug toolbar.  I know we can collapse it, but it's still in the way even then.
                var debugSpacer = new Label { Text = "Debug Toolbar Spacer ", HeightRequest = 48, HorizontalOptions = LayoutOptions.Center };
                MainGrid.Children.Add(debugSpacer);
                Grid.SetColumnSpan(debugSpacer, 2);

                // Heinous Hack-Stockings!
                // cjm - My God this is a Horrible Hack to get around broken Maui HeadTruncate Controls.  
                //       Worse it only updates when you run a new search to redraw the list.
                //       We could probably put an event in for on width change. 
                FilePathFontSize = 8;
                FilePathFontColor = "Black";
                new Task(async () =>
                {
                    while (AngelHornetLogo.Height < 1) await Task.Delay(25);
                    while (true)
                    {
                        // Read-Only so it should be fine this way.
                        FilePathWindowWidth = int.Max((int)TestSonglist.Width, Const.AppMinimumWidth);
                        await Task.Delay(3000);
                    }
                }, TaskCreationOptions.LongRunning).Start();
            }

            _ = DeliverMessageQueue(_messageQueue, spinBox, messageBox);
            _ = SecondWindow(Application.Current, AngelHornetLogo);
            if (Const.UseSongCache)
            {
                _ = PlaySongTask();
                _ = CacheSongTask();
            }

            // Load Databases
            {
                LogDebug("Loading Saved Database");
                var _playlists = _dbContext.Playlists.ToList();
                _playlists = _playlists.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
                TestPlaylist.ItemsSource = _playlists;
                var _songList = _dbContext.Songs.ToList();
                _songList = _songList.OrderBy(s => s.AlphaTitle, StringComparer.OrdinalIgnoreCase).ToList();
                TestSonglist.ItemsSource = _songList;
                if (_playlists.Count < 1) UpdateAllSongsPlaylist().Wait();  // cj this won't take effect till we re-load, but it's better than nothing.
                LogMsg($"Loaded {_playlists.Count} Playlists, and {_songList.Count} Songs.");
                LogDebug("=== /Database Loading Complete =============================== ===");
            }

            // Search for More Music
            Task.Run(async () =>
            {
                var _progress = new ReportProgressToQueue(_messageQueue);

                await SearchUserProfileMusic(_progress);
                await UpdateAllSongsPlaylist();

                LogDebug("Database Dispatch Start.");
                var _playlists = _dbContext.Playlists.ToList();
                _playlists = _playlists.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
                this.Dispatcher.Dispatch(() =>
                               TestPlaylist.ItemsSource = _playlists);

                var _songList = _dbContext.Songs.ToList();
                _songList = _songList.OrderBy(s => s.AlphaTitle, StringComparer.OrdinalIgnoreCase).ToList();
                this.Dispatcher.Dispatch(() =>
                               TestSonglist.ItemsSource = _songList);
                await Task.Delay(1);
                LogDebug("=== /Database Dispatch Complete =============================== ===");
                await Task.Delay(3000);
                LogMsg("Startup Complete");
            });

 

        }  // /Constructor

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

        private async void TestSonglist_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var _song = (Song)e.SelectedItem;
            var _index = e.SelectedItemIndex;
            var _listView = (ListView)sender;
            _listView.Focus();
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
            if (_newSong == null && _repeatPlaylist && _selectedSongIndex >= _sourceSongList.Count - 1) // cjm
            {
                LogMsg($"Repeating Playlist");
                _newSong = _sourceSongList.ElementAtOrDefault(0);
            }
            var _newSongIndex = _sourceSongList.IndexOf(_newSong);
            var _title = _newSong != null ? _newSong.Title : "null";

            LogMsg($"ChangeSong:{_selectedSongIndex}->{_newSongIndex}/{_sourceSongList.Count-1 }: {_title}");
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

        private void ChangePlaylist(Playlist playlist)
        {
            if (playlist != null) LogMsg($"ChangePlaylist: {playlist.Id}   {playlist.Name}   {playlist.Description}");
            else LogWarning("WARN[124] playlist is null");

            var _songList = _dbContext.Songs.Where(s => s.Playlists.Contains(playlist)).ToList();
            _songList = _songList.OrderBy(s => s.AlphaTitle, StringComparer.OrdinalIgnoreCase).ToList();
            this.Dispatcher.Dispatch(() =>
            {
                TestSonglist.ItemsSource = _songList;
                TestSonglist.SelectedItem = _songList.FirstOrDefault();
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
                    if (File.Exists(_cachedPath)) _mediaSource = MediaSource.FromFile(_cachedPath);            // cjm 
                    else if (File.Exists(_cachedPath)) _mediaSource = MediaSource.FromFile(_cachedPath);            // cjm 
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
                LogError("ERROR: Song is null");
                return;
            }




            // Clear the Queue
            _cacheSongQueue.Clear();
            _cacheSongQueue.Enqueue(null); // signal the queue to stop downloads

            // (Re)Fill the Queue, Queue it up fast, don't check the file systems, that just slows it all down.
            {
                for (int i = _index; i < _list.Count && i < _index + Const.QueueSize; i++)
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
            var _cacheCleanDelay = Const.CacheCleanInterval / 10;
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
                    LogTrace(" SongCacheTask is Waiting");
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
                        LogDebug($"Cleaning Cache: {_fileCount} files removed.");
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
                    LogTrace("  SongPlayTask is Waiting");
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
            _songList = _songList.OrderBy(s => s.AlphaTitle, StringComparer.OrdinalIgnoreCase).ToList();
            _dbContext.Dispose();

            LogMsg($"Found {_songList.Count} songs.");
            if (_songList.Count > 0)
                Application.Current.Dispatcher.Dispatch(() =>
                {
                    TestSonglist.ItemsSource = _songList;
                });
        }

        private void Shuffle_Clicked(object sender, EventArgs e)
        {
            LogMsg("Shuffle");
            var button = (Button)sender;
            var _songList = TestSonglist.ItemsSource.Cast<Song>().ToArray();
            new Random().Shuffle(_songList);
            var _list = _songList.ToList();
            Application.Current.Dispatcher.Dispatch(() =>
            {
                TestSonglist.ItemsSource = _list;
            });
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
                Application.Current.Dispatcher.Dispatch( () => TestSonglist.SelectedItem = _newSong );
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
