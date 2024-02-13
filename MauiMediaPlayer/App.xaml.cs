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

        const int SCALE = 720;
        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);
            // if(OperatingSystem.IsWindows())  // cjm - might need this if it causes issues on other platforms
            // Change the window Size
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            window.Width = SCALE; window.Height = SCALE;

            // Fit Win 11 Height
            int borders = 48;
            int width = (int) (SCALE * 9 / 16);         // 16:9 aspect ratio Widescreen 
            int height = SCALE;
            int phoneHeight = (int) (2.22 * width);     // 2.22 is the aspect ratio of a typical phone
            int maxDisplayHeight = (int) (displayInfo.Height / displayInfo.Density - borders); // * displayInfo.Density;
            if (phoneHeight < maxDisplayHeight) height = phoneHeight;
            else height = maxDisplayHeight;
            window.Width = width;
            window.Height = height;

            // BONUS -> Center the window
            window.X = (displayInfo.Width / displayInfo.Density - window.Width) / 2;
            window.Y = (displayInfo.Height / displayInfo.Density - window.Height) / 2 - borders/2;

            Debug.WriteLine($"\n************\nDisplay Info: {displayInfo.Width}x{displayInfo.Height} * {displayInfo.Density}");
            Debug.WriteLine($"Window Info: {window.Width}x{window.Height} @ {window.X}x{window.Y}\n");
            return window;
        }
    }



}

