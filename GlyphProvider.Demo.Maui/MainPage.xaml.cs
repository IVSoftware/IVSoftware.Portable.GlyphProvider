using IVSoftware.Portable;
using System.Diagnostics;
using Font = Microsoft.Maui.Font;

namespace IVSGlyphProvider.Demo.Maui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            // This lowers the lazy init time
            GlyphProvider.BoostCache();
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
            Debug.WriteLine(string.Empty);
            Debug.WriteLine(
                string.Join(
                    $"{Environment.NewLine}{Environment.NewLine}",
                    prototypes));
            Debug.Assert(prototypes.Any(), "Did you set config.json to Embedded resource?");

            var fontFamily = typeof(IconBasics).ToCssFontFamilyName();
            { }
#endif
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
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
                    // Works independently
                    // CounterBtn.FontFamily = typeof(IconBasics).ToCssFontFamilyName();

                    // Also works, but this must be aliased in Maui.AddFont
                    CounterBtn.FontFamily = nameof(IconBasics);

                    CounterBtn.Text = IconBasics.Search.ToGlyph();
                    CounterBtn.WidthRequest = CounterBtn.Height;
                    // Alt
                    var xaml = IconBasics.Search.ToGlyph(GlyphFormat.Xaml);
                    // Readable
                    var display = IconBasics.Search.ToGlyph(GlyphFormat.UnicodeDisplay);
                    { }
                    //var fonts = GlyphProvider.ListDomainFontResources();
#if false
                CounterBtn.FontFamily = "basics-icons";
                CounterBtn.Text = CounterBtn.FontFamily.ToGlyph(IconBasics.Search);
                CounterBtn.WidthRequest = CounterBtn.Height;
                // Alt
                var xaml = CounterBtn.FontFamily.ToGlyph(IconBasics.Search, GlyphFormat.Xaml);
                // Readable
                var display = CounterBtn.FontFamily.ToGlyph(IconBasics.Search, GlyphFormat.UnicodeDisplay);
                { }

                var fonts = IVSoftware.Portable.GlyphProvider.ListDomainFontResources();


                var names = GetType().Assembly.GetManifestResourceNames();
                { }
                SemanticScreenReader.Announce(CounterBtn.Text);
                stopwatch.Stop();
                Debug.WriteLine(stopwatch.ElapsedTicks);
                // If preloaded (call to ToGlyph()) 640675 ticks
                // Otherwise as much as            2764151
#endif
                }
        }
        private readonly string _fontFamilyPrev;
        private readonly double _widthRequestPrev;
        int count = 0;
    }
}