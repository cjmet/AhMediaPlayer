using AhConfig;
using static AngelHornetLibrary.AhLog;




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
