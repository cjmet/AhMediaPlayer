using CommunityToolkit.Maui.Views;
using DataLibrary;
using System.Diagnostics;
using static CommonNet8.SearchForMusic;
using static AngelHornetLibrary.AhLog;
using Windows.Media.Playlists;
using Windows.Media.MediaProperties;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using AngelHornetLibrary;



namespace MauiMediaPlayer
{
    public partial class MyPage : ContentPage
    {

        public MyPage()
        {
            InitializeComponent();



            List<string> debugStrings = new List<string>();
            // open a readonly file stream asynchronously
            Task.Run(async () =>
            {
                var fileName = AhLog._logFilePath;
                if (File.Exists(fileName))
                    LogInfo($"File Exists: {fileName}");
                else
                    LogInfo($"File Does Not Exist: {fileName}");
                Task.Delay(1000).Wait(); // - cjm - remove this later.
                using (FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    LogInfo($"FileStream Opened: {fileStream.Name}");
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        LogInfo($"StreamReader Opened.");
                        string line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            debugStrings.Add(line);
                        }
                        LogInfo($"StreamReader Completed.");
                        if (debugStrings.Count > 0)
                        {
                            Application.Current.Dispatcher.Dispatch(() =>
                                {
                                    Debuglist.ItemsSource = debugStrings;
                                    Debuglist.SelectedItem = debugStrings.LastOrDefault();
                                    Debuglist.ScrollTo(debugStrings.LastOrDefault(), ScrollToPosition.Start, false);
                                });
                        }
                    }
                    LogInfo($"FileStream Completed.");
                }
            });

            //Task.Run(async () =>
            //{
            //    Task.Delay(3000).Wait();
            //    Application.Current.Dispatcher.Dispatch(() =>
            //                     Debuglist.ItemsSource = debugStrings);
            //});

            // //StreamReader
            // //StreamReader
        }

    }
}
