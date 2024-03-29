﻿using AngelHornetLibrary;
using static AngelHornetLibrary.AhLog;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using Microsoft.Maui.Controls.Platform;
using System.Reflection.Metadata;


namespace MauiMediaPlayer
{

    public class DataStruct
    {
        public string DataString { get; set; }
    }

    
    public partial class MyPage : ContentPage
    {
        //public List<DataStruct> pageOfData { get; set; } = new List<DataStruct>();
        public ObservableCollection<string> pageOfData { get; set; } = new ObservableCollection<string>();


        public MyPage()
        {

            InitializeComponent();

            // Notes:
            // ---
            // * Webview has it's own scroll!  ...  of course it does, and of course it's not (easily) controlled by the same methods as the rest of the app.
            // * HtmlWebViewSource does not support JavaScript.  But it does auto-reload. This results in reloading jumping to the top, half a second later java jumping to the bottom, repeat.  Hilarious, but not useful.  Under other circumstances HtmlWebViewSource would be the way to go.
            //    HtmlWebViewSource htmlSource = new HtmlWebViewSource();
            //    htmlSource.Html = "Temporary    BODY    Place Holder Text";
            //    webView.Source = htmlSource; 


            {
                var logFile = AhLog._logFilePath;
                if (!File.Exists(logFile))
                    throw new FileNotFoundException($"Logfile Does Not Exist: {logFile}");
                var htmlFile = logFile + ".html";
                var urlFile = "file:///" + htmlFile;
                LogInfo("Log Viewer WebView Window Starting:");
                LogInfo($"   logFile: {logFile}\n   htmlFile: {htmlFile}\n   urlFile: {urlFile}");

                WebView webView = new WebView();
                this.Content = webView;
                webView.Source = urlFile;

                Task _ = Task.Run(async () =>
                {
                    List<string> allLines = new List<string>();
                    List<string> newLines = new List<string>();
                    string header =
"""
<!DOCTYPE html>
<html>
<body">
""";
                    // ---  -----------
                    string footer =
"""
</body>
</html>
""";
                    // ---  -----------
                    int fileSize = 0;
                    int filePos = 0;
                    while (await this.Dispatcher.DispatchAsync(() => webView.Height) < 1) await Task.Delay(1000);
                    do
                    {
                        //Do Stuff Here!
                        Debug.WriteLine($"Preparing to Read File");
                        // Yeap, we're dieing here. Need FileShare.ReadWrite - cjm 
                        newLines = File.ReadAllLines(logFile).Skip(filePos).ToList();

                        Debug.WriteLine($"File Read: {newLines.Count} lines");
                        filePos += newLines.Count;
                        fileSize = (int)new FileInfo(logFile).Length;
                        if (newLines != null)
                        {
                            allLines.AddRange(newLines);
                            var body = String.Join("<br>\n", allLines);
                            body = String.Join("<br>\n", header, body,  footer);

                            // Testing Height did not work here.
                            var isTmp = await this.Dispatcher.DispatchAsync(() => webView.IsEnabled && webView.IsVisible && webView.IsLoaded);
                            if (!isTmp) { Debug.WriteLine($"WebView is Not Available"); return; throw new Exception(); }

                            Debug.WriteLine($"Dispatching Update to htmlSource");
                            File.WriteAllLines(htmlFile, new string[] { body });
                            await this.Dispatcher.DispatchAsync(() =>
                                {
                                    webView.Reload();
                                    webView.EvaluateJavaScriptAsync("window.scrollTo(0, document.body.scrollHeight);");
                                }); 
                            Debug.WriteLine($"WebView Reloaded");
                        }
                        await Task.Delay(1000); // cjm 
                        //while (fileSize == (int)new FileInfo(fileName).Length) await Task.Delay(100000); // cjm 
                    } while (true); // cjm 
                });
            }



            // *** OBSOLETE CODE BELOW *** ... But I'm keeping it for reference.

            // *** OBSOLETE CODE BELOW *** ... But I'm keeping it for reference.

            // *** OBSOLETE CODE BELOW *** ... But I'm keeping it for reference.



            // ========================================
            // Stream Reader
            // 
            // MUST use ObserverableCollection, and MUST use Dispatcher.DispatchAsync, (or at least use {Dispatch(); await Task.Delay(1)}, when running on a different thread.
            // ObservableCollection is a collection that provides notifications when items get added, removed, or when the whole list is refreshed.  This makes updates automatic.
            // Dispatcher is a class that provides services for managing the queue of work items for a thread, and allows for communication between threads.
            // MUST use FileShare.ReadWrite to allow the file to be read while it is open by the other process. ReadWrite is a bit counterintuitive, as I'm only asking for read access, but it is necessary.
            // Permissions by level?: Read, Write, ReadWrite, Delete, Append, AllAccess
            // Increase permission level until you get the access you need.
            // *** await Application.Current.Dispatcher.DispatchAsync() !!!!   Is definitely the method you want, particularly when running async on a different thread.
            //        Alternatively you 'can' {Dispatch(); await Task.Delay(1);} in order to allow the system to process the Dispatch before continuing.  However, this is slower, a little buggy, and is not recommended.
            // This seems to have Worked.  Wait for the DataWindow to be created before continuing.
            // It also has the side effect of allowing the entire songdb to dispatch first as well.
            //  while (DataWindow.Height < 1) { await Task.Delay(25); waited += 25; }

            return;
            LogInfo("Start of Initial Dispatch Testing");
            var data = "Debugging Window Starting ...";
            pageOfData.Add(data);
            this.Dispatcher.Dispatch(() => DataWindow.ItemsSource = pageOfData);
            pageOfData.Add("Test Data Line 2");
            //Task.Run(() => { pageOfData.Add("Test Data Line 3"); });        // Safe only because it's on the same thread?  Even then I'm not entirely sure.
            Task.Run(async () => { await this.Dispatcher.DispatchAsync(() => pageOfData.Add("Test Data Line 4")); });   // MUST use the DispatchAsync
            LogInfo("End of Initial Dispatch Testing");

            LogInfo("Breakpoint [67]");
            Task.Run(async () =>
            {
                var waited = 0;
                // This seems to have Worked.  Wait for the DataWindow to be created before continuing.
                // It also has the side effect of allowing the entire songdb to dispatch first as well.
                while (DataWindow.Height < 1) { await Task.Delay(25); waited += 25; }
                LogInfo($"DataWindow.Height: {(int)DataWindow.Height}  Waited: {waited}");

                // Using Debug.WriteLine inside here because we do NOT want an infinite loop of messages.
                var fileName = AhLog._logFilePath;
                if (File.Exists(fileName))
                    LogInfo($"FileStream Target Exists: {fileName}");
                else
                {
                    LogInfo($"FileStream Target Does Not Exist: {fileName}");
                    throw new FileNotFoundException($"FileStream Target Does Not Exist: {fileName}");
                }

                int filePos = 0;
                int fileSize = 0;

                do
                {
                    using (FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        fileStream.Seek(filePos, SeekOrigin.Begin);
                        //Debug.WriteLine($"FileStream SavedPos: {filePos}   CurrentPos: {fileStream.Position}");
                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            string line;
                            while ((line = await reader.ReadLineAsync()) != null)
                            {
                                await this.Dispatcher.DispatchAsync(() =>
                                    pageOfData.Add(line));
                                await this.Dispatcher.DispatchAsync(() =>
                                    DataWindow.ScrollTo(line, ScrollToPosition.End, false));
                            }
                            filePos = (int)fileStream.Position;
                            fileSize = (int)fileStream.Length;
                        }
                    }
                    int onDiskSize = 0;
                    do
                    {
                        await Task.Delay(1000);
                        onDiskSize = (int)new FileInfo(fileName).Length;
                        if (onDiskSize > fileSize) Debug.WriteLine($"FileStream File Size Changed: {onDiskSize} > {fileSize}");
                    } while (onDiskSize <= fileSize);
                } while (true);
            });
            //
            // ^StreamReader
            // ========================================
        }

    }
}
