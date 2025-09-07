using IVSoftware.Portable;

namespace GlyphProvider.Demo.Maui
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

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
                // CounterBtn.WidthTracksHeight = false;
            }
            else
            {
                CounterBtn.FontFamily = "onepage-icons";

                //    // This is what the Glyph property abstracts!
                //    // OLD
                //    // CounterBtn.Glyph = GlyphProvider.FromFontConfigJson(Framework.OP_FONT_FAMILY)["Search"];
                //    // NEW
                //    CounterBtn.Glyph = Framework.OP_FONT_FAMILY.ToGlyph(StdOnepageIconsGlyph.Search);
                // Alt
                var xaml = CounterBtn.FontFamily.ToGlyph(StdBasicsIcons.Search, GlyphFormat.Xaml);
                // Readable
                var display = CounterBtn.FontFamily.ToGlyph(StdBasicsIcons.Search, GlyphFormat.UnicodeDisplay);
                { }

                //    CounterBtn.WidthTracksHeight = true;
                //}

                SemanticScreenReader.Announce(CounterBtn.Text);
            }
        }
    }
}