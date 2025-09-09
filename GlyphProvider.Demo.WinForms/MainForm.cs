using IVSoftware.Portable;
using System;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace IVSGlyphProvider.Demo.WinForms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
#if DEBUG
            if(GlyphProvider.TryGetFontsDirectory(out string? dir, allowCreate: true))
            {
                _ = GlyphProvider.CopyEmbeddedFontsFromPackage(dir);
            }
#endif
            GlyphProvider.BoostCache<IconBasics>();



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
                CounterBtn.Font = BasicsFont;
                CounterBtn.Text = CounterBtn.Font.Name.ToGlyph(IconBasics.Search);
                CounterBtn.Width = CounterBtn.Height;
                // Alt
                var xaml = CounterBtn.Font.Name.ToGlyph(IconBasics.Search, GlyphFormat.Xaml);
                // Readable
                var display = CounterBtn.Font.Name.ToGlyph(IconBasics.Search, GlyphFormat.UnicodeDisplay);
                { }
                var fonts = IVSoftware.Portable.GlyphProvider.ListDomainFontResources();
            }
        }
        private readonly int _widthRequestPrev;
        private readonly Font _fontPrev;

        public Font BasicsFont
        {
            get
            {
                var fontName = "basics-icons.ttf";
                var alias = "basics-icons";
                if (_basicsFont is null)
                {
                    var asm = typeof(IVSoftware.Portable.GlyphProvider).Assembly;
                    var fullName = 
                        asm
                        .GetManifestResourceNames()                        
                        .First(_ => _.EndsWith(fontName, StringComparison.OrdinalIgnoreCase));

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
                        FontFamily fontFamily = CustomFonts.Families.Single(_ => _.Name == alias);
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
