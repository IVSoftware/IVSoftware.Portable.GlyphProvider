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
                var window = base.CreateWindow(activationState);
                window.Width = 450;
                window.Height = 850;

                // Ensure window resizing completes
                window.Dispatcher.DispatchAsync(() => { }).GetAwaiter().OnCompleted(() =>
                {
                    var disp = DeviceDisplay.Current.MainDisplayInfo;
                    window.X = (disp.Width / disp.Density - window.Width) / 2;
                    window.Y = (disp.Height / disp.Density - window.Height) / 2;
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
