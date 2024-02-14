using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AngelHornetLibrary.CLI;
using DataLibrary;
using AngelHornetLibrary;
using CommunityToolkit.Maui.Views;





namespace MauiMediaPlayer
{
    public partial class MainPage : ContentPage
    {

        int count = 10;

        private readonly PlaylistContext _dbContext;
        //List<string> result = new List<string>();
        //string searchStatus = "Searching...";


        public MainPage(PlaylistContext dbcontext)
        //public MainPage()
        {
            InitializeComponent();
            _dbContext = dbcontext;



            //=== ======================================
            //vvv vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

            // Callback for FindFilesConcurrentQueue
            Action<ConcurrentQueue<string>, bool> callback = (ConcurrentQueue<string> q, bool doneFlag) =>
            {
                string r;

                var _adbContext = new PlaylistContext();
                while (q.TryDequeue(out r))
                {
                    Debug.WriteLine($"*** DeQueueing: {r}");
                    var _dirName = Path.GetDirectoryName(r);
                    var _fileName = Path.GetFileNameWithoutExtension(r);
                    Debug.WriteLine($"*** Adding: {r}");
                    _adbContext.Songs.Add(new Song
                    {
                        Title = _fileName,
                        PathName = r,
                        Comment = _dirName
                    });
                    Debug.WriteLine($"*** Added: {r}");
                }
                Debug.WriteLine(" *** Saving Changes: _adbContext ***");
                _adbContext.SaveChanges();
                Debug.WriteLine(" *** Saved ***");

                if (_adbContext.Songs.Count() > 0 || doneFlag)
                {
                    
                    Debug.WriteLine(" *** _adbContext.Songs.Count() > 0 ***");
                    // cjm - StringComparer.OrdinalIgnoreCase is causing a crash here
                    // changed to query into a tmp List, then sort the tmp List, then set the ItemSource
                    //_adbContext.Songs.OrderBy(s => s.Title));   
                    Debug.WriteLine(" *** Dispatch Playlists to UI");
                    // cjm - Add Code Here Later, if needed, and if this is the right place for it?
                    Debug.WriteLine(" *** Doh, there are none, because I cleared them out.  Rewrite this later and insert it.");
                    // ---
                    Debug.WriteLine(" *** Dispatch Songs to UI");
                    var _songs = _adbContext.Songs.ToList();
                    _songs = _songs.OrderBy(s => s.Title, StringComparer.OrdinalIgnoreCase).ToList();
                    Application.Current.MainPage.Dispatcher.Dispatch(() =>
                        TestSonglist.ItemsSource = _songs);

                    Application.Current.MainPage.Dispatcher.Dispatch(() =>
                        messageText.Text = $"{_adbContext.Songs.Count().ToString()} Songs Found");

                    // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

                    if (doneFlag)
                    {
                        _songs = _adbContext.Songs.ToList();
                        _songs = _songs.OrderBy(s => s.Title, StringComparer.OrdinalIgnoreCase).ToList();
                        var _song = _songs.FirstOrDefault();
                        
                        Debug.WriteLine("\n=== ======================================== ===\n");
                        Debug.WriteLine($"Attempting to set _mediaSource = {_song.PathName}");
                        Debug.WriteLine("\n=== ======================================== ===\n");
                        MediaSource _mediaSource;
                        // cjm - this is causing a crash here
                        if (_song.PathName != null && File.Exists(_song.PathName))
                        {
                            _mediaSource = MediaSource.FromFile(_song.PathName);
                            if (_mediaSource != null)
                            {
                                mediaElement.Source = _mediaSource;
                                Application.Current.MainPage.Dispatcher.Dispatch(() =>
                                    mediaTitle.Text = _song.Title);
                                Application.Current.MainPage.Dispatcher.Dispatch(() =>
                                    mediaArtist.Text = $"{_song.Artist} - {_song.Album}");
                            }
                        }
                        // /cjm - this is causing a crash here
                    }
                    // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                }
                //_adbContext.Dispose();  // cjm - this is causing a crash ... so don't dispose of it, let the program do it for me?
            };
            // /Callback for FindFilesConcurrentQueue

            {
                List<string> paths = new List<string> { "C:\\users\\cjmetcalfe\\Music", "c:\\users\\khaai\\Music" };
                Debug.WriteLine("\n=== ======================================== ===\n");
                Debug.WriteLine("FindFilesConcurrentQueue ... ");
                foreach (var _path in paths)
                    Debug.WriteLine($"::> {_path}");
                Debug.WriteLine("\n=== ======================================== ===\n");

                new FindFilesConcurrentQueue(callback, paths, "*.mp3", SearchOption.AllDirectories);
            }

            //^^^ ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            //=== ======================================



            TestPlaylist.ItemsSource = _dbContext.Playlists.ToList();
            TestSonglist.ItemsSource = _dbContext.Songs.ToList();



            // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            // ==========================================================
            // Test Background Tasks .... 


            //Task.Run(() =>
            //{
            //    while (true)
            //    {
            //        Task.Delay(1000).Wait();
            //        count++;
            //        Application.Current.MainPage.Dispatcher.Dispatch(() => counterText.Text = count.ToString()); //  String.Format("Downloading {0}%", count)); 
            //    }
            //});


            //    Task task;
            //    //List<string> targetDirs = ["C:\\users\\cjmetcalfe\\Music"];
            //    List<string> targetDirs = ["C:\\users\\cjmetcalfe\\music", "c:\\users\\khaai\\music",];
            //    task = new Task(() =>
            //    {
            //        foreach (var dir in targetDirs)
            //        {
            //            Debug.WriteLine($"\n*** Searching *** \n::> {dir}\n");
            //            new AhsUtil().GetFilesRef(dir, "*.mp3", ref result, ref searchStatus, SearchOption.AllDirectories);
            //        }
            //    }, TaskCreationOptions.LongRunning);
            //    task.Start();


            //    Task.Run(() =>
            //    {
            //        var spinner = 0;
            //        do
            //        {
            //            //string tmp = searchStatus;
            //            //if (tmp == null) tmp = "null";
            //            var tmp = $"{searchStatus}";
            //            var maxLength = 60;
            //            if (tmp.Length > maxLength) tmp = "" + tmp.Substring(tmp.Length - maxLength, maxLength);
            //            var spin = "|/-\\"[spinner++ % 4];
            //            tmp = $"[{result.Count}] {spin} {tmp}";
            //            Application.Current.MainPage.Dispatcher.Dispatch(() => pathText.Text = tmp);
            //            Task.Delay(250).Wait();
            //        } while (task.Status == TaskStatus.Running);
            //        Application.Current.MainPage.Dispatcher.Dispatch(() => pathText.Text = $"Found [{result.Count}] Mp3 files.");
            //    });



            // ==========================================================
            // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            //if (count == 1)
            //    CounterBtn.Text = $"Clicked {count} time";
            //else
            //    CounterBtn.Text = $"Clicked {count} times";
            //SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }

}
