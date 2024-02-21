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
using WinRT;
using Microsoft.Maui.Layouts;



namespace MauiMediaPlayer
{
    public partial class MyPage : ContentPage
    {

        public MyPage()
        {
            InitializeComponent();



            List<string> debugStrings = new List<string>();
            debugStrings.Add("Debugging Window Starting ...");
            Application.Current.Dispatcher.Dispatch(() =>
                Debuglist.ItemsSource = debugStrings);
            // open a readonly file stream asynchronously
            Task.Run(async () =>
            {
                var fileName = AhLog._logFilePath;
                if (File.Exists(fileName))
                    LogInfo($"FileStream Target  Exists: {fileName}");
                else
                    LogInfo($"FileStream Target Does Not Exist: {fileName}");
                await Task.Delay(3000); // - cjm - remove this later.
                Debug.WriteLine($"FileStream Opening: {fileName}");
                using (FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    Debug.WriteLine($"FileStream Opened: {fileStream.Name}");
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        Debug.WriteLine($"StreamReader Opened.");
                        string line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            debugStrings.Add(line);
                        }
                        Debug.WriteLine($"StreamReader Completed.");
                        if (debugStrings.Count > 0)
                        {
                            Debug.WriteLine($"FileStream Dispatching Updates");
                            //cjm - it's not updating unless I clear and recreate the list
                            // may be forced to use a data class with one member
                            Debug.WriteLine($"FileStream Dispatching Update [60]");
                            Application.Current.Dispatcher.Dispatch(() =>
                                Debuglist.ItemsSource = null);
                            Debug.WriteLine($"FileStream Dispatching Update [63]");
                            Application.Current.Dispatcher.Dispatch(() =>
                                Debuglist.ItemsSource = debugStrings);
                            // cjm - we have to wait for the list to be populated before we can scroll 
                            // cjm - is there a way to ask the window if it's done populating?
                            // cjm - var _sourceSongList = TestSonglist.ItemsSource.Cast\<Song>().ToList();
                            var delay = TimeSpan.FromMilliseconds(2500);
                            await Task.Delay(delay);
                            //List<string> _source;
                            Debug.WriteLine($"FileStream Dispatching Update [71]");
                            //Debug.WriteLine(Debuglist.ItemsSource.GetType().ToString());
                            //_source = Debuglist.ItemsSource.Cast<string>().ToList();
                            Debug.WriteLine($"FileStream Dispatching Update [72]");
                            //do
                            //{
                            //    _source = Debuglist.ItemsSource.Cast<string>().ToList();
                            //    await Task.Delay(25);
                            //} while (_source.Count != debugStrings.Count);
                            var target = debugStrings.LastOrDefault();
                            if (target != null)
                            {
                                Debug.WriteLine($"FileStream Dispatching Update [77]");
                                Application.Current.Dispatcher.Dispatch(
                                    () => Debuglist.SelectedItem = debugStrings.LastOrDefault());
                                Debug.WriteLine($"FileStream Dispatching Update [80]");
                                Application.Current.Dispatcher.Dispatch(
                                    () => Debuglist.ScrollTo(debugStrings.LastOrDefault(), ScrollToPosition.End, true));
                            }
                            Debug.WriteLine($"FileStream Dispatching Complete");
                        }
                    }
                    Debug.WriteLine($"FileStream Completed.");
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
