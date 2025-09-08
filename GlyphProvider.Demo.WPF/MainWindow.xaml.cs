using IVSoftware.Portable;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GlyphProvider.Demo.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            localInitSizeAsRawPixels();
			// This lowers the lazy init time
			IVSoftware.Portable.GlyphProvider.BoostCache();
			_fontFamilyPrev = CounterBtn.FontFamily;
			_widthPrev = CounterBtn.Width;

			#region L o c a l F x       
            void localInitSizeAsRawPixels()
            {
                var dpi = VisualTreeHelper.GetDpi(this);

                this.Width *= (96.0 / dpi.PixelsPerInchX);
                this.Height *= (96.0 / dpi.PixelsPerInchY);
			}	
            #endregion L o c a l F x
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
				CounterBtn.FontFamily = new FontFamily("basics-icons");
				CounterBtn.Content = CounterBtn.FontFamily.Source.ToGlyph(StdBasicsIcons.Search);
				CounterBtn.Width = CounterBtn.Height;
#if false
				// Alt
				var xaml = CounterBtn.FontFamily.N.ToGlyph(StdBasicsIcons.Search, GlyphFormat.Xaml);
				// Readable
				var display = CounterBtn.FontFamily.ToGlyph(StdBasicsIcons.Search, GlyphFormat.UnicodeDisplay);
				{ }

				var fonts = IVSoftware.Portable.GlyphProvider.ListDomainFontResources();


				var names = GetType().Assembly.GetManifestResourceNames();
				{ }
				stopwatch.Stop();
				Debug.WriteLine(stopwatch.ElapsedTicks);
				// If preloaded (call to ToGlyph()) 640675 ticks
				// Otherwise as much as            2764151
#endif
			}

		}
		private readonly FontFamily _fontFamilyPrev;
		private readonly double _widthPrev;
		int count = 0;
	}
}