using RepeatSegment.Maui.Services;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace RepeatSegment.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>()
               .UseSkiaSharp()
               .ConfigureFonts(fonts =>
               {
                   fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                   fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
               });

#if ANDROID
        PlaybackServiceManager.Init(Android.App.Application.Context);
#endif
        return builder.Build();
    }
}
