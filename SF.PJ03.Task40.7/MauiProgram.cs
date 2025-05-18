using Microsoft.Extensions.Logging;
using SF.PJ03.Task40._7_.Models;

namespace SF.PJ03.Task40._7_
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
                });
            // Регистрация сервисов
            RegisterServices(builder.Services);

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        // Регистрирует сервисы приложения в контейнере зависимостей.
        private static void RegisterServices(IServiceCollection services)
        {
#if ANDROID
            services.AddTransient<IGalleryService, SF.PJ03.Task40._7_.Platforms.Android.Services.AndroidGalleryService>();
#else
            services.AddTransient<IGalleryService, DefaultGalleryService>();
#endif
        }
    }
}