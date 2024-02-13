using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AngelHornetLibrary.CLI;
using DataLibrary;





namespace MauiMediaPlayer
{
    public partial class MainPage : ContentPage
    {

        int count = 0;

        private readonly PlaylistContext _dbContext;

        List<string> result = new List<string>();
        string searchStatus = "Searching...";

        public MainPage(PlaylistContext dbcontext)
        //public MainPage()
        {
            InitializeComponent();
            _dbContext = dbcontext;
//#if DEBUG
//            const string dbName = "test_playlists.db";
//            var folder = Environment.SpecialFolder.LocalApplicationData;
//            var path = Environment.GetFolderPath(folder);
//            var DbPath = Path.Join(path, dbName);
//            Debug.WriteLine($"\n**********************");
//            Debug.WriteLine($"MAUI  DbPath: {DbPath}");
//            Debug.WriteLine($"\n**********************\n");
//#endif
        
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


            Task task;
            //List<string> targetDirs = ["C:\\users\\cjmetcalfe\\Music"];
            List<string> targetDirs = ["C:\\users\\cjmetcalfe\\music", "c:\\users\\khaai\\music",  ];
            task = new Task(() =>
            {
                foreach (var dir in targetDirs)
                {
                    Debug.WriteLine($"\n*** Searching *** \n::> {dir}\n");
                    new AhsUtil().GetFilesRef(dir, "*.mp3", ref result, ref searchStatus, SearchOption.AllDirectories);
                }
            }, TaskCreationOptions.LongRunning);
            task.Start();


            Task.Run(() =>
            {
                var spinner = 0;
                do
                {
                    //string tmp = searchStatus;
                    //if (tmp == null) tmp = "null";
                    var tmp = $"{searchStatus}";
                    var maxLength = 60;
                    if (tmp.Length > maxLength) tmp = "" + tmp.Substring(tmp.Length - maxLength, maxLength);
                    var spin = "|/-\\"[spinner++ % 4];
                    tmp = $"[{result.Count}] {spin} {tmp}";
                    Application.Current.MainPage.Dispatcher.Dispatch(() => pathText.Text = tmp);
                    Task.Delay(250).Wait();
                } while (task.Status == TaskStatus.Running);
                Application.Current.MainPage.Dispatcher.Dispatch(() => pathText.Text = $"Found [{result.Count}] Mp3 files.");
            });

        }
        // ==========================================================
        // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv


        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }

}
