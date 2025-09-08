using IVSoftware.Portable;
using System.Diagnostics;

namespace GlyphProvider.Demo.Maui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            //var xaml = CounterBtn.FontFamily.ToGlyph(StdBasicsIcons.Search, GlyphFormat.Xaml);
            _ = typeof(IVSoftware.Portable.GlyphProvider).Assembly;
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;
            if (count % 2 == 0)
            {
                var cycleCount = count / 2;
                CounterBtn.FontFamily = "OpenSans-Regular";
                if (cycleCount == 1)
                    CounterBtn.Text = $"Cycled {cycleCount} time";
                else
                    CounterBtn.Text = $"Cycled {cycleCount} times";
                if(_widthRequestPrev is double width)
                {
                    CounterBtn.WidthRequest = width;
                }
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();
                CounterBtn.FontFamily = "basics-icons";
                CounterBtn.Text = CounterBtn.FontFamily.ToGlyph(StdBasicsIcons.Search);
                _widthRequestPrev = CounterBtn.WidthRequest;
                CounterBtn.WidthRequest = CounterBtn.Height;
                // Alt
                var xaml = CounterBtn.FontFamily.ToGlyph(StdBasicsIcons.Search, GlyphFormat.Xaml);
                // Readable
                var display = CounterBtn.FontFamily.ToGlyph(StdBasicsIcons.Search, GlyphFormat.UnicodeDisplay);
                { }

                var fonts = IVSoftware.Portable.GlyphProvider.ListFonts();


                var names = GetType().Assembly.GetManifestResourceNames();
                { }
                SemanticScreenReader.Announce(CounterBtn.Text);
                stopwatch.Stop();
                Debug.WriteLine(stopwatch.ElapsedTicks);
                // If preloaded (call to ToGlyph()) 640675 ticks
                // Otherwise as much as            2764151
            }

        }
        double? _widthRequestPrev;
        int count = 0;
    }
}