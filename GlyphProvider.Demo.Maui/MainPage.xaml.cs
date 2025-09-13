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
            // CenteringPanel.Configure<ToolbarButtons>();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            string announce;
            switch (CounterBtn.Text)
            {
                default:
                    // SAVE: Works independently
                    // CounterBtn.FontFamily = typeof(IconBasics).ToCssFontFamilyName();

                    // ALIASED: This must be set up in Maui.AddFont
                    CounterBtn.FontFamily = nameof(IconBasics);
                    CounterBtn.WidthRequest = CounterBtn.Height;
                    CounterBtn.Text = IconBasics.HelpCircledAlt.ToGlyph();
                    CenteringPanel.Configure<ToolBarEmpty>();
                    announce = nameof(IconBasics.HelpCircledAlt);
                    break;
                case string s when s == IconBasics.HelpCircledAlt.ToGlyph():
                    CounterBtn.Text = IconBasics.EllipsisHorizontal.ToGlyph();
                    announce = nameof(IconBasics.EllipsisHorizontal);
                    break;
                case string s when s == IconBasics.EllipsisHorizontal.ToGlyph():
                    CounterBtn.Text = IconBasics.EllipsisVertical.ToGlyph();    // Staging vertical as next action.
                    CenteringPanel.Configure<ToolbarButtons>();                 // While responding to the horizontal click.
                    announce = nameof(IconBasics.EllipsisVertical);
                    break;
                case string s when s == IconBasics.EllipsisVertical.ToGlyph():
                    CounterBtn.WidthRequest = _widthRequestPrev;
                    CounterBtn.FontFamily = _fontFamilyPrev;
                    CounterBtn.Text =
                        count == 0
                        ? $"Cycled {++count} time"
                        : $"Cycled {++count} times";

                    // Here is where we're overriding the default one way or another.
                    Debug.Assert(DateTime.Now.Date == new DateTime(2025, 9, 13).Date, "Don't forget disabled");

                    CenteringPanel.Configure<ToolbarButtons>(
                        orientation: LayoutOrientation.Vertical,
                        // displayFormatOptions: DisplayFormatOptions.ShowIcon
                        // displayFormatOptions: DisplayFormatOptions.ShowSquareIconAndMember
                        displayFormatOptions: DisplayFormatOptions.ShowMember
                    );
                    announce = CounterBtn.Text;
                    break;
            }
            SemanticScreenReader.Announce(announce);
        }
        private readonly string _fontFamilyPrev;
        private readonly double _widthRequestPrev;
        int count = 0;
    }
}