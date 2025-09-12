using IVSoftware.Portable;
using IVSoftware.Portable.Demo;
using System.Diagnostics;
using Font = Microsoft.Maui.Font;

namespace IVSGlyphProvider.Demo.Maui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            _fontFamilyPrev = CounterBtn.FontFamily;
            _widthRequestPrev = CounterBtn.WidthRequest;
            _ = InitAsync();
        }

        private async Task InitAsync()
        {
            // Reduce the lazy "first time click" latency.
            await GlyphProvider.BoostCache();
#if DEBUG
            if (GlyphProvider.TryGetFontsDirectory(out string? dir, allowCreate: true))
            {
                _ = GlyphProvider.CopyEmbeddedFontsFromPackage(dir);
            }
            var prototypes = await GlyphProvider.CreateEnumPrototypes();
            Debug.Assert(
                prototypes.Any(), 
                "At the very least, you should see the IconBasics prototype." +
                "You should also see any config.json files marked as Embedded Resource." +
                "Even for WPF this should be Embedded Resource not Resource."
           );

            var showMe =
                string.Join(
                    $"{Environment.NewLine}{Environment.NewLine}",
                    prototypes);
            var expected = @"".Trim();

            var fontFamily = typeof(IconBasics).ToCssFontFamilyName();
            { }
#endif
            CenteringPanel.Configure<ToolbarButtons>();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            switch (CounterBtn.Text)
            {
                default:
                    // SAVE: Works independently
                    // CounterBtn.FontFamily = typeof(IconBasics).ToCssFontFamilyName();

                    // ALIASED: This must be set up in Maui.AddFont
                    CounterBtn.FontFamily = nameof(IconBasics);
                    CounterBtn.WidthRequest = CounterBtn.Height;
                    CounterBtn.Text = IconBasics.HelpCircledAlt.ToGlyph();
                    break;
                case string s when s == IconBasics.HelpCircledAlt.ToGlyph():
                    CounterBtn.Text = IconBasics.EllipsisHorizontal.ToGlyph();
                    break;
                case string s when s == IconBasics.EllipsisHorizontal.ToGlyph():
                    CounterBtn.Text = IconBasics.EllipsisVertical.ToGlyph();
                    break;
                case string s when s == IconBasics.EllipsisVertical.ToGlyph():
                    CounterBtn.WidthRequest = _widthRequestPrev;
                    CounterBtn.FontFamily = _fontFamilyPrev;
                    CounterBtn.Text =
                        count == 0
                        ? $"Cycled {++count} time"
                        : $"Cycled {++count} times";
                    break;
            }

#if false
            var stopwatch = Stopwatch.StartNew();
            count++;
            if (count % 2 == 0)
            {
                var cycleCount = count / 2;
                if (cycleCount == 1)
                    CounterBtn.Text = $"Cycled {cycleCount} time";
                else
                    CounterBtn.Text = $"Cycled {cycleCount} times";
                CounterBtn.WidthRequest = _widthRequestPrev;
            }
            else
            {
                string expected;

                // Works independently
                // CounterBtn.FontFamily = typeof(IconBasics).ToCssFontFamilyName();

                // Also works, but this must be aliased in Maui.AddFont
                CounterBtn.FontFamily = nameof(IconBasics);

                CounterBtn.Text = IconBasics.HelpCircledAlt.ToGlyph();
                CounterBtn.WidthRequest = CounterBtn.Height;
                // Alt
                var xaml = IconBasics.Search.ToGlyph(GlyphFormat.Xaml);
                expected = "&#xE807;";
                // Readable
                var display = IconBasics.Search.ToGlyph(GlyphFormat.UnicodeDisplay);
                expected = "U+E807";
                { }
            }
            stopwatch.Stop();
#if false
            SemanticScreenReader.Announce(CounterBtn.Text);
            Debug.WriteLine(stopwatch.ElapsedTicks);
            // If preloaded (call to ToGlyph()) 640675 ticks
            // Otherwise as much as            2764151
#endif
#endif
        }
        private readonly string _fontFamilyPrev;
        private readonly double _widthRequestPrev;
        int count = 0;
    }
}