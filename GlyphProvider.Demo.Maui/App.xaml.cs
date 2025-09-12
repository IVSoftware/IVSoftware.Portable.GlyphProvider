namespace IVSGlyphProvider.Demo.Maui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new MainPage();
        }
        protected override Window CreateWindow(IActivationState? activationState)
        {
            if (Application.Current is not null && DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                var window = base.CreateWindow(activationState); var disp = DeviceDisplay.Current.MainDisplayInfo;

                // Intended pixel size ~~ WinForms and WPF
                double targetPixelWidth = 518;
                double targetPixelHeight = 904;

                // Convert pixels → DIPs
                window.Width = targetPixelWidth / disp.Density;
                window.Height = targetPixelHeight / disp.Density;

                // Center on screen in DIPs
                window.Dispatcher.DispatchAsync(() =>
                {
                    var screenWidthDip = disp.Width / disp.Density;
                    var screenHeightDip = disp.Height / disp.Density;

                    window.X = (screenWidthDip - window.Width) / 2;
                    window.Y = (screenHeightDip - window.Height) / 2;
                });
                return window;
            }
            else
            {
                return base.CreateWindow(activationState);
            }
        }
    }
}
