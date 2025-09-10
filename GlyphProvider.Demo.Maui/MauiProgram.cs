﻿using Microsoft.Extensions.Logging;

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
                    fonts.AddFont("icon-basics.ttf", "icon-basics");
                    //fonts.AddEmbeddedResourceFont(typeof(IVSoftware.Portable.GlyphProvider).Assembly,"icon-basics.ttf", "icon-basics")  ;
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
