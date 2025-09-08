using IVSoftware.Portable;

namespace GlyphProvider.Demo.Maui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
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

                SemanticScreenReader.Announce(CounterBtn.Text);
            }
        }
        double? _widthRequestPrev;
        int count = 0;
    }
}