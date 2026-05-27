using Microsoft.Extensions.Logging;
<<<<<<< Updated upstream
=======
using Microsoft.Extensions.DependencyInjection;// AddHttpClient için şart
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;
using TeacherFind.Mobile.App;
using TeacherFind.Mobile.Core.Abstractions;
using TeacherFind.Mobile.Core.Services;
using TeacherFind.Mobile.Core.Storage;
using TeacherFind.Mobile.Core.Handlers;
using TeacherFind.Mobile.Features.Auth.Views;
using TeacherFind.Mobile.Features.Auth.ViewModels;
using System.Net.Http;
>>>>>>> Stashed changes

namespace TeacherFind.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<MainApp>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // 1. Storage & Session (Public olduklarından emin oluyoruz)
        builder.Services.AddSingleton<ITokenStorage, TokenStorageService>();
        builder.Services.AddSingleton<IUserSession, UserSessionService>();
        builder.Services.AddTransient<AuthHeaderHandler>();

        // 2. HTTP Client Altyapısı
        builder.Services.AddTransient<IApiService, ApiService>(); // Arayüz ve sınıfı doğrudan eşleştiriyoruz

        builder.Services.AddHttpClient("TeacherFindApi", client =>
        {
<<<<<<< Updated upstream
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
=======
            client.BaseAddress = new Uri("https://10.0.2.2:7001/");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<AuthHeaderHandler>(); // Token ekleme motorunu güvenle bağladık

        // 3. Servis Kayıtları
        builder.Services.AddSingleton<IAuthService, AuthService>();

        // 4. Auth Modülü Ekranları
        builder.Services.AddTransient<SplashPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ForgotPasswordPage>();
        builder.Services.AddTransient<ResetPasswordPage>();

        builder.Services.AddTransient<SplashViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ForgotPasswordViewModel>();
        builder.Services.AddTransient<ResetPasswordViewModel>();
>>>>>>> Stashed changes

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}