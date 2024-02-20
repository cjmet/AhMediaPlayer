using Microsoft.Maui.Controls.Platform.Compatibility;
using System.Diagnostics;
using static AngelHornetLibrary.AhLog;

namespace MauiMediaPlayer
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        
        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);
            // if(OperatingSystem.IsWindows())  // cjm - might need this if it causes issues on other platforms
            // Change the window Size
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            window.Width = Const.AppWidth; window.Height = Const.AppHeight;

            // Fit Win 11 Height
            int maxDisplayHeight = (int) (displayInfo.Height / displayInfo.Density - Const.AppDisplayBorder); // * displayInfo.Density;
            if (window.Height > maxDisplayHeight) window.Height = maxDisplayHeight;
            
            // BONUS -> Center the window
            window.X = (displayInfo.Width / displayInfo.Density - window.Width) / 2;
            window.Y = (displayInfo.Height / displayInfo.Density - window.Height) / 2 - Const.AppDisplayBorder/2;

            window.MinimumWidth = Const.AppMinimumWidth; window.MinimumHeight = Const.AppMinimumHeight;
            window.MaximumWidth = Const.AppMaximumWidth;
            LogInfo("\n=== ======================================== ===\n");

            LogInfo($"*** Display Info: {displayInfo.Width}x{displayInfo.Height} * {displayInfo.Density}");
            LogInfo($"*** Window Info: {window.Width}x{window.Height} / {window.MinimumWidth}x{window.MinimumHeight} / {window.MinimumWidth} @ {window.X}x{window.Y}");

            LogInfo("\n=== /Display Info =============================== ===\n");



            return window;
        }
    }



}

