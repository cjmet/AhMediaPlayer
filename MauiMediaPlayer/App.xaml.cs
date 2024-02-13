using Microsoft.Maui.Controls.Platform.Compatibility;
using System.Diagnostics;

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


            Debug.WriteLine("\n=== ======================================== ===\n");

            Debug.WriteLine($"*** Display Info: {displayInfo.Width}x{displayInfo.Height} * {displayInfo.Density}");
            Debug.WriteLine($"*** Window Info: {window.Width}x{window.Height} @ {window.X}x{window.Y}");

            Debug.WriteLine("\n=== /Display Info =============================== ===\n");



            return window;
        }
    }



}

