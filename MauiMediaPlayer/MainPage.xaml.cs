using System.Threading.Tasks;
using AngelHornetLibrary.CLI;




namespace MauiMediaPlayer
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        List<string> result = new List<string>();
        string searchStatus = "Searching...";

        public MainPage()
        {

            InitializeComponent();

            Task.Run(() =>
            {
                while (true)
                {
                    Task.Delay(1000).Wait();
                    count++;
                    Application.Current.MainPage.Dispatcher.Dispatch(() => counterText.Text = count.ToString()); //  String.Format("Downloading {0}%", count)); 
                }
            });


            Task task = new Task(() => { new AhsUtil().GetFilesRef("C:\\users", "*.mp3", ref result, ref searchStatus, SearchOption.AllDirectories); }, TaskCreationOptions.LongRunning);
            task.Start();

            Task.Run(() =>
            {
                do
                {
                    string tmp = searchStatus;
                    if (tmp == null) tmp = "null";
                    tmp = $"[{result.Count,10}] {result.LastOrDefault()} \nSearching: {tmp}";
                    Application.Current.MainPage.Dispatcher.Dispatch(() => pathText.Text = tmp);
                    Task.Delay(250).Wait();
                } while (task.Status == TaskStatus.Running);
                Application.Current.MainPage.Dispatcher.Dispatch(() => pathText.Text = "Finished Searching.");
            });
        }

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
