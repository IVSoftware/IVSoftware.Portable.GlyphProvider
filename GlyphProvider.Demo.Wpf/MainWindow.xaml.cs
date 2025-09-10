using IVSoftware.Portable;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Media;

namespace IVSGlyphProvider.Demo.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
#if DEBUG
            if (GlyphProvider.TryGetFontsDirectory(out string? dir, allowCreate: true))
            {
                _ = GlyphProvider.CopyEmbeddedFontsFromPackage(dir);
            }
            { }
#endif
            InitializeComponent();
            _ = InitAsync();

            #region L o c a l F x       
            void localInitSizeAsRawPixels()
            {
                var dpi = VisualTreeHelper.GetDpi(this);

                this.Width *= (96.0 / dpi.PixelsPerInchX);
                this.Height *= (96.0 / dpi.PixelsPerInchY);
            }
            #endregion L o c a l F x
        }

        private async Task InitAsync()
        {
            // Reduce the lazy "first time click" latency.
            await GlyphProvider.BoostCache();
            _fontFamilyPrev = CounterBtn.FontFamily;
            _widthPrev = CounterBtn.Width;
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
            var expected = @"[CssName(""icon-basics"")]
public enum StdIconBasics
{
	[CssName(""add"")]
	Add

	[CssName(""delete"")]
	Delete

	[CssName(""edit"")]
	Edit

	[CssName(""ellipsis-horizontal"")]
	EllipsisHorizontal

	[CssName(""ellipsis-vertical"")]
	EllipsisVertical

	[CssName(""filter"")]
	Filter

	[CssName(""menu"")]
	Menu

	[CssName(""search"")]
	Search

	[CssName(""settings"")]
	Settings

	[CssName(""checked"")]
	Checked

	[CssName(""unchecked"")]
	Unchecked

	[CssName(""eye"")]
	Eye

	[CssName(""eye-off"")]
	EyeOff

	[CssName(""help-circled"")]
	HelpCircled

	[CssName(""help-circled-alt"")]
	HelpCircledAlt

	[CssName(""doc-empty"")]
	DocEmpty

	[CssName(""doc"")]
	Doc

	[CssName(""doc-new"")]
	DocNew
}".Trim();

            Debug.Assert(showMe == expected, "ADVISORY: This will only hold true until more config.json files are added!");

            var fontFamily = typeof(IconBasics).ToCssFontFamilyName();
            expected = "icon-basics";
            Debug.Assert(fontFamily == expected);

            fontFamily = typeof(IconBasics).ToCssFontFamilyName(ext: "ttf");
            expected = "icon-basics.ttf";
            Debug.Assert(fontFamily == expected);

            fontFamily = typeof(IconBasics).ToCssFontFamilyName(ext: ".ttf");
            expected = "icon-basics.ttf";
            Debug.Assert(fontFamily == expected);
#endif
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;
            if (count % 2 == 0)
            {
                var cycleCount = count / 2;
                CounterBtn.FontFamily = new FontFamily("Open Sans");
                if (cycleCount == 1)
                    CounterBtn.Content = $"Cycled {cycleCount} time";
                else
                    CounterBtn.Content = $"Cycled {cycleCount} times";
                if (_widthPrev is double width)
                {
                    CounterBtn.Width = width;
                }
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();
                CounterBtn.FontFamily = IconBasicsFontFamily;
                CounterBtn.Content = IconBasics.Search.ToGlyph();
                CounterBtn.Width = CounterBtn.Height;

				// Alt
                var xaml = IconBasics.Search.ToGlyph(GlyphFormat.Xaml);
				// Readable
                var display = IconBasics.Search.ToGlyph(GlyphFormat.UnicodeDisplay);
				stopwatch.Stop();
				Debug.WriteLine(stopwatch.ElapsedTicks);
            }
        }
        private FontFamily? _fontFamilyPrev;
        private double _widthPrev;
        int count = 0;

        public static FontFamily IconBasicsFontFamily
        {
            get
            {
                if (_iconBasicsFontFamily is null)
                {
                    // WPF programmatic font load
                    // We do not expect this to work because in GlyphProvider
                    // the 'icon-basics.ttf' file is an embedded resource.
                    // var asmName = typeof(IVSoftware.Portable.GlyphProvider).Assembly.GetName().Name;

                    // WPF programmatic font load
                    // On the other hand, in this assembly the 'icon-basics.ttf' file is marked Resource.
                    var asm = typeof(MainWindow).Assembly;


                    // This URI looks in Resources/Fonts/icon-basics.ttf
                    // (assuming Build Action = Resource, not EmbeddedResource)
                    var fontUri = new Uri(
                        $"{PackApplicationBaseUri}{asm.GetName().Name};component/{
                            asm.GetResourcePathMatch(endsWith: typeof(IconBasics).ToCssFontFamilyName(ext: ".ttf"), @throw: true)}",
                        UriKind.Absolute);

                    // The string after # must match the internal font face name declared in the TTF
                    _iconBasicsFontFamily = new FontFamily(fontUri, $"./#{typeof(IconBasics).ToCssFontFamilyName()}");
                }
                return _iconBasicsFontFamily;
            }
        }
        static FontFamily? _iconBasicsFontFamily = null; 
        private const string PackApplicationBaseUri = "pack://application:,,,/";
    }

    static class PlatformExtensions
    {
        public static string GetResourcePathMatch(this Assembly asm, string endsWith, bool @throw = false)
        {
            // Open the compiled WPF .g.resources
            var resName = asm.GetName().Name + ".g.resources";
            using var stream = asm.GetManifestResourceStream(resName);
            if (stream is null) return string.Empty;

            using var reader = new ResourceReader(stream);
            return
                reader
                .Cast<System.Collections.DictionaryEntry>()
                .Select(_=>_.Key as string)
                .Where(_=>_ is not null)
                .SingleOrDefault(_ => _!.EndsWith(endsWith, StringComparison.InvariantCultureIgnoreCase)) 
                ??
                (@throw
                    ? throw new NullReferenceException()
                    : string.Empty);
        }
    }
}