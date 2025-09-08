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
            void localInitSizeAsRawPixels()
            {
                var dpi = VisualTreeHelper.GetDpi(this);

                this.Width *= (96.0 / dpi.PixelsPerInchX);
                this.Height *= (96.0 / dpi.PixelsPerInchY);
            }
        }
    }
}