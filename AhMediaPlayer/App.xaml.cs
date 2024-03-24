using AhConfig;
using static AngelHornetLibrary.AhLog;

namespace AhMediaPlayer
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
            // if(OperatingSystem.IsWindows())  // cj - might need this if it causes issues on other platforms
            // Change the window Size
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            window.Width = Const.AppWidth; window.Height = Const.AppHeight;

            // Fit Win 11 Height
            int maxDisplayHeight = (int)(displayInfo.Height / displayInfo.Density - Const.AppDisplayBorder); // * displayInfo.Density;
            if (window.Height > maxDisplayHeight) window.Height = maxDisplayHeight;

            // BONUS -> Center-ish the window
            var minXLoc = 1280 - Const.AppWidth - Const.AppDisplayBorder / 2;
            var minDisplayXLoc = (int)(displayInfo.Width / displayInfo.Density - Const.AppWidth - Const.AppDisplayBorder / 2);
            window.X = int.Min(minXLoc, minDisplayXLoc);
            if (window.X < 1152 - Const.AppWidth - Const.AppDisplayBorder / 2)
                window.X = 1152 - Const.AppWidth - Const.AppDisplayBorder / 2;
            window.Y = (displayInfo.Height / displayInfo.Density - window.Height) / 2 - Const.AppDisplayBorder / 2;

            window.MinimumWidth = Const.AppMinimumWidth; window.MinimumHeight = Const.AppMinimumHeight;
            window.MaximumWidth = Const.AppMaximumWidth;

            LogDebug($"*** Display Info: {displayInfo.Width}x{displayInfo.Height} * {displayInfo.Density}");
            LogDebug($"*** Window Default:{window.Width}x{window.Height}   Min:{window.MinimumWidth}x{window.MinimumHeight}   Max:{window.MaximumWidth}w   Loc:{window.X}x{window.Y}");

            LogDebug("=== /Display Info =============================== ===");



            return window;
        }
    }



}

