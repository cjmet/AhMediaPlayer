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



            Action<ConcurrentQueue<string>, bool> callback = (ConcurrentQueue<string> q, bool d) =>
            {
                string r; while (q.TryDequeue(out r))
                {
                    Debug.WriteLine(r);
                    var _dirName = Path.GetDirectoryName(r);
                    var _fileName = Path.GetFileNameWithoutExtension(r);
                    _dbContext.Songs.Add(new Song
                    {
                        Title = _fileName,
                        PathName = r,
                        Comment = _dirName
                    });
                }
                dbcontext.SaveChanges();
                var _songs = _dbContext.Songs.ToList();
                _songs = _songs.OrderBy(s => s.Title, StringComparer.OrdinalIgnoreCase).ToList();
                Application.Current.MainPage.Dispatcher.Dispatch(() =>
                    TestSonglist.ItemsSource = _songs);
                // cjm - StringComparer.OrdinalIgnoreCase is causing a crash here
                // changed to query into a tmp List, then sort the tmp List, then set the ItemSource
                //_dbContext.Songs.OrderBy(s => s.Title));   

                Application.Current.MainPage.Dispatcher.Dispatch(() =>
                    pathText.Text = $"{_dbContext.Songs.Count().ToString()} Songs Found.");



                // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
                Task.Delay(25).Wait();
                string _filename = _dbContext.Songs.FirstOrDefault ().PathName;
                Debug.WriteLine("\n=== ======================================== ===\n");
                Debug.WriteLine($"_filename = {_filename}");
                Debug.WriteLine("\n=== ======================================== ===\n");
                MediaSource _mediaSource;
                if (_filename != null && File.Exists(_filename))
                {
                    _mediaSource = MediaSource.FromFile(_filename);
                    if (_mediaSource != null)
                    {
                        mediaElement.Source = _mediaSource;
                    }
                }
                // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^


            };



            List<string> paths = new List<string> { "C:\\users\\cjmetcalfe\\Music", "c:\\users\\khaai\\Music" };
            new FindFilesConcurrentQueue(callback, paths, "*.mp3", SearchOption.AllDirectories);



            //^^^ ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            //=== ======================================


                
            TestPlaylist.ItemsSource = _dbContext.Playlists.ToList();
            TestSonglist.ItemsSource = _dbContext.Songs.ToList();


            
            // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            // ==========================================================
            // Test Background Tasks .... 


            Task.Run(() =>
            {
                while (true)
                {
                    Task.Delay(1000).Wait();
                    count++;
                    Application.Current.MainPage.Dispatcher.Dispatch(() => counterText.Text = count.ToString()); //  String.Format("Downloading {0}%", count)); 
                }
            });


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
