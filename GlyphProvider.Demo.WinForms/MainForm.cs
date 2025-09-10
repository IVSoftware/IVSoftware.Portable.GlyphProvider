using IVSoftware.Portable;
using System;
using System.Diagnostics;
using System.Drawing.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace IVSGlyphProvider.Demo.WinForms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            _fontPrev = CounterBtn.Font;
            _widthRequestPrev = CounterBtn.Width;
            CounterBtn.UseCompatibleTextRendering = true;
            CounterBtn.SizeChanged += (sender, e) =>
            {
                var containerWidth = Width - Padding.Horizontal;
                var btnWidth = CounterBtn.Width + CounterBtn.Margin.Horizontal;
                CounterBtn.Left = (containerWidth - btnWidth) / 2;
            };
            CounterBtn.Click += OnCounterClicked;
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
            { }

            var fontFamily = typeof(IconBasics).ToCssFontFamilyName();
            { }
#endif
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;
            if (count % 2 == 0)
            {
                var cycleCount = count / 2;
                if (cycleCount == 1)
                    CounterBtn.Text = $"Cycled {cycleCount} time";
                else
                    CounterBtn.Text = $"Cycled {cycleCount} times";
                    CounterBtn.Width = _widthRequestPrev;
            }
            else
            {
                CounterBtn.Font = IconBasicsFont;
                CounterBtn.Text = IconBasics.Search.ToGlyph();
                CounterBtn.Width = CounterBtn.Height;
                // Alt
                var xaml = IconBasics.Search.ToGlyph(GlyphFormat.Xaml);
                // Readable
                var display = IconBasics.Search.ToGlyph(GlyphFormat.UnicodeDisplay);
                { }
                //var fonts = GlyphProvider.ListDomainFontResources();
            }
        }
        private readonly int _widthRequestPrev;
        private readonly Font _fontPrev;

        public Font IconBasicsFont
        {
            get
            {
                if (_basicsFont is null)
                {
                    var glyphProvider = GlyphProvider.GetProvider<IconBasics>()
                        ?? throw new NullReferenceException();

                    string
                        cssName = glyphProvider.Name,
                        fileName = $"{cssName}.ttf";
                    var asm = typeof(MainForm).Assembly;
                    var fullName = 
                        asm
                        .GetManifestResourceNames()                        
                        .FirstOrDefault(_ => _.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

                    if(string.IsNullOrWhiteSpace(fullName))
                    {
                        throw new InvalidOperationException("Embedded resource not found.");
                    }

                    using (Stream fontStream = asm.GetManifestResourceStream(fullName)
                           ?? throw new InvalidOperationException($"Failed to load stream for '{fullName}'."))
                    {
                        byte[] fontData = new byte[fontStream.Length];
                        fontStream.Read(fontData, 0, fontData.Length);

                        IntPtr fontPtr = Marshal.AllocCoTaskMem(fontData.Length);
                        try
                        {
                            Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
                            CustomFonts.AddMemoryFont(fontPtr, fontData.Length);
                        }
                        finally
                        {
                            Marshal.FreeCoTaskMem(fontPtr); // Avoid memory leak
                        }
                        FontFamily fontFamily = CustomFonts.Families.Single(_ => _.Name == cssName);
                        _basicsFont = new Font(fontFamily, _fontPrev.Size);
                    }
                }
                return _basicsFont;
            }
        }
        Font? _basicsFont = null;

        public PrivateFontCollection CustomFonts
        {
            get
            {
                if (_customFonts is null)
                {
                    _customFonts = new PrivateFontCollection();
                }
                return _customFonts;
            }
        }
        PrivateFontCollection? _customFonts = null;

        int count = 0;
    }
}
