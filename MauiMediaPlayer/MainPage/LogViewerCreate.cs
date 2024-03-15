using AhConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Microsoft.EntityFrameworkCore;






namespace MauiMediaPlayer
{
    public partial class MainPage : ContentPage
    {


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


    }
}
