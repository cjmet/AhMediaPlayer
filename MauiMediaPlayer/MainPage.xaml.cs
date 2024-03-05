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
        public static int FilePathFontMargin { get => -int.Max(FilePathFontSize / 2 - 2, 1); }
        public static string FilePathFontColor = "Transparent";
        public static int FilePathWindowWidth = 385;
        // /Hack-Stockings



        // Constructor Main Page
        public MainPage(PlaylistContext dbcontext)
        //public MainPage()
        {
            // Change: Application.Current.MainPage.Dispatcher.Dispatch
            // to: this.Dispatcher.Dispatch

            InitializeComponent();
            _dbContext = dbcontext;

            _ = DeliverMessageQueue(_messageQueue, spinBox, messageBox);
            _ = SecondWindow(Application.Current, AngelHornetLogo);

            if (Const.UseSongCache)
            {
                _ = PlaySongTask();
                _ = CacheSongTask();
            }

            if (Debugger.IsAttached)
            {
                // Hack to space stuff down below the debug toolbar.  I know we can collapse it, but it's still in the way even then.
                var debugSpacer = new Label { Text = "Debug Toolbar Spacer ", HeightRequest = 48, HorizontalOptions = LayoutOptions.Center };
                MainGrid.Children.Add(debugSpacer);
                Grid.SetColumnSpan(debugSpacer, 2);
            }

            // I went to a lot of trouble to create this, and now after input from others I'm going to make it permanent instead of a DEBUG_ONLY feature.
            // Heinous Hack-Stockings!
            // cj - My God this is a Horrible Hack to get around broken Maui HeadTruncate Controls.  
            //       Worse it only updates when you run a new search to redraw the list.
            //       We could probably put an event in for on width change. 
            FilePathFontSize = 8;
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
                TestSonglist.ItemsSource = _songList;
                if (_playlists.Count < 1) UpdateAllSongsPlaylist().Wait();  // cj this won't take effect till we re-load, but it's better than nothing.
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
                    list = CommonAdvancedSearch.ShortHelpText();
                    AdvancedSearchHelpList.ItemsSource = list;
                });
            });

            // Search for More Music
            Task.Run(async () =>
            {
                var _progress = new ReportProgressToQueue(_messageQueue);

                await SearchUserProfileMusic(_progress);
                await UpdateAllSongsPlaylist();

                LogDebug("Database Dispatch Start");
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
                await Task.Delay(1000);
                LogMsg("Startup Complete");
            });

        }  // /Constructor /Main Page



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
                "angel_hornet_logo_cropped.png", "azrael.png",
                "bad_grim.png", "baxters.png", "big_moon.png",
                "brie.png", "bubby.png", "chex.png", "damnit_gizmo.png",
                "django.png", "dot.png", "fox.png", "herc.png",
                "howard.png", "huggy.png", "kibbs.png", "kissy.png",
                "lucy.png", "luna.png", "nino.png", "pie.png",
                "possum.png", "rey.png", "spicey.png", "stella.png",
                "tiger.png"
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
                _ = SearchUserProfileMusic(_progress, _path);
            }
        }

        private void MenuBox_Clicked(object sender, EventArgs e)
        {
            if (AdvandedSearchFrame.IsVisible) Application.Current.Dispatcher.Dispatch(() =>
            {
                AdvandedSearchFrame.IsVisible = false;
                MenuBox.BackgroundColor = Color.Parse("Transparent");
            });
            else Application.Current.Dispatcher.Dispatch(() =>
            {
                AdvandedSearchFrame.IsVisible = true;
                MenuBox.BackgroundColor = Color.Parse("LightBlue");
                Searchby.SelectedIndex = 0;
                SearchAction.SelectedIndex = 0;
            });

        }

        private async void SearchBar_SearchButtonPressed(object sender, EventArgs e) => AdvancedSearchBar_SearchButtonPressed(sender, e);
        private void AdvancedSearchBar_SearchButtonPressed(object sender, EventArgs e)
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

            var _db = new PlaylistContext();
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


            //(_advancedResult, _searchBy, _searchAction) = CommonAdvancedSearch.AdvancedSearchParse(_currentSet, _searchText, _by, _action);  // cjm 
            (_advancedResult, _searchBy, _searchAction) = AdvancedSearch(_currentSet, _searchText, _by, _action);
            if (_advancedResult != null)
            {
                _songList = _advancedResult.ToList();
                if (_searchBy != null) _by = _searchBy;
                if (_searchAction != null) _action = _searchAction;
            }

            if (_searchAction != "IS" && _advancedResult != null) RandomPersistentLogo(_searchText, _songList.Count);

            Application.Current.Dispatcher.Dispatch(() =>
            {
                if (_songList.Count > 0)
                {
                    TestSonglist.ItemsSource = _songList;
                    SearchCount.Text = $"{_songList.Count:n0}";
                }
                Searchby.SelectedItem = _searchBy;
                SearchAction.SelectedItem = _searchAction;
                var Placeholder = "Search Title, Artist, Band, Album, Genre, or Path";      // cjm
                if (_by != null && _by != "Any") Placeholder = $"SearchBy: {_searchBy}     -     SearchAction: {_searchAction}";
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