﻿using AngelHornetLibrary;
using static AngelHornetLibrary.AhLog;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using Microsoft.Maui.Controls.Platform;
using System.Reflection.Metadata;
using Windows.UI.Text;
using System.ComponentModel;


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

            // Concept:  This was the Plan
            //WebView webView = new WebView();
            //webView.Source = AhLog._logFilePath;
            //this.Content = webView;
            //_ = Task.Run(async () =>
            //{
            //    while (true)
            //    {
            //        await Task.Delay(1000);
            //        await this.Dispatcher.DispatchAsync(() => webView.Reload());
            //    } 
            //});
            //return;



            // Reality:  This is what I got, after three tries of various solutions:  ListView, WebView, ScrollView.
            // <Log Viewer>
            {
                var logFile = AhLog._logFilePath;
                if (!File.Exists(logFile))
                    throw new FileNotFoundException($"Logfile Does Not Exist: {logFile}");

                ScrollView scrollView = new ScrollView();
                StackLayout stackLayout = new StackLayout();
                var logText = "LogView Text:\n";
                Label label = new Label() { Text = logText, FontSize = 11 };
                this.Content = scrollView;
                scrollView.Content = stackLayout;
                stackLayout.Children.Add(label);

                _ = Task.Run(async () =>
                {
                    while (await this.Dispatcher.DispatchAsync(() => label.Height) < 1) await Task.Delay(1000);
                    await Task.Delay(1);  // ~1 GUI cycle
                    this.Dispatcher.Dispatch(async () => await scrollView.ScrollToAsync(label, ScrollToPosition.End, false));
                    await Task.Delay(1); // ~1 GUI cycle

                    int i = 0;
                    var pollLinesAsync = new AhFileIO().PollLinesAsync(logFile);
                    await foreach (var line in pollLinesAsync)
                    {
                        this.Dispatcher.Dispatch(async () =>
                        {
                            logText += line + "\n";
                            label.Text = logText;
                            await Task.Delay(1);    // This gives multi-line updates ~1 GUI cycle to update so the scroll moves to the end of the last line.  Otherwise it only moves to the first line of the update.
                            await scrollView.ScrollToAsync(label, ScrollToPosition.End, false);
                        });
                    }
                });
            }
            // </Log Viewer>
        }
    }
}