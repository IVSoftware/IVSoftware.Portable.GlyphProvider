using Microsoft.Extensions.Logging;

namespace GlyphProvider.Demo.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {

#if DEBUG
            var fontsDir = "D:\\Github\\IVSoftware\\TMP\\IVSoftware.Portable.GlyphProvider\\GlyphProvider.Demo.Maui\\Resources\\Fonts\\";
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            // "D:\Github\IVSoftware\TMP\IVSoftware.Portable.GlyphProvider\GlyphProvider.Demo.Maui\bin\Debug\net8.0-windows10.0.19041.0\win10-x64\AppX\
            { }
#endif


            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("basics-icons.ttf", "basics-icons");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
