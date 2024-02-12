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
            window.Width = 720; window.Height = 720;

            // Fit Win 11 Height
            var borders = 48;
            window.Height = displayInfo.Height / displayInfo.Density - borders; // * displayInfo.Density;

            // 16:9 Aspect Ratio, Portrait Mode, This will be similar to a phone  Phones are 2.22h instead of 1.78h
            window.Width = window.Height * 9 / 16;

            // BONUS -> Center the window
            window.X = (displayInfo.Width / displayInfo.Density - window.Width) / 2;
            window.Y = (displayInfo.Height / displayInfo.Density - window.Height) / 2 - borders/2;

            Debug.WriteLine($"\n************\nDisplay Info: {displayInfo.Width}x{displayInfo.Height} * {displayInfo.Density}");
            Debug.WriteLine($"Window Info: {window.Width}x{window.Height} @ {window.X}x{window.Y}\n");
            return window;
        }
    }



}

