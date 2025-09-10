using IVSoftware.Portable;
using Microsoft.Extensions.Logging;

namespace IVSGlyphProvider.Demo.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

#if !USE_LOCAL_COPY
                    fonts.AddEmbeddedResourceFont(
                        typeof(IVSoftware.Portable.GlyphProvider).Assembly,
                        filename: $"{typeof(IconBasics).ToCssFontFamilyName()}.ttf",
                        alias: $"{nameof(IconBasics)}");
#else
                    // Another alternative is:
                    // FIRST call this method while in DEBUG mode:
                    //  GlyphProvider.CopyEmbeddedFontsFromPackage(dir);
                    // THEN
                    // - Manually set the Build Action for the local 'icon-basics.ttf' to MauiFont
                    fonts.AddFont("icon-basics.ttf", "icon-basics");
#endif
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
