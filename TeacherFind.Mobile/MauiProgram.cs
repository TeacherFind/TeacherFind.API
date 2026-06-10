using Microsoft.Extensions.Logging;
using System.Net.Http; // HttpClient için ekledik!
using TeacherFind.Mobile.App;

namespace TeacherFind.Mobile
{
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

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // =======================================================
            // 1. ÇEKİRDEK SERVİS KAYITLARI (Core Services)
            // =======================================================
            builder.Services.AddSingleton<HttpClient>(); // İnternete çıkış aracımız (Eksik olan parça)
            builder.Services.AddSingleton<TeacherFind.Mobile.Core.Abstractions.IApiService, TeacherFind.Mobile.Core.Services.ApiService>();


            // =======================================================
            // 2. KABUK VE MENÜ KAYITLARI (Shared Components)
            // =======================================================
            builder.Services.AddTransient<TeacherFind.Mobile.Shared.Components.MainFlyoutMenuPage>();
            builder.Services.AddSingleton<TeacherFind.Mobile.Shared.Components.MainShellPage>();


            // =======================================================
            // 3. SAYFA VE MOTOR KAYITLARI (Features)
            // =======================================================

            // --- Auth Modülü ---
            builder.Services.AddTransient<TeacherFind.Mobile.Features.Auth.Views.LoginPage>();
            builder.Services.AddTransient<TeacherFind.Mobile.Features.Auth.Views.RegisterPage>();

            // --- Ana Sayfa Modülü (Home) ---
            builder.Services.AddTransient<TeacherFind.Mobile.Features.Home.Views.HomePage>();
            builder.Services.AddTransient<TeacherFind.Mobile.Features.Home.ViewModels.HomeViewModel>();

            // --- Öğretmen Arama Modülü (Teachers) ---
            builder.Services.AddTransient<TeacherFind.Mobile.Features.Teachers.Views.TeacherListPage>();
            builder.Services.AddTransient<TeacherFind.Mobile.Features.Teachers.ViewModels.TeacherListViewModel>();

            // --- Profil Modülü (Profile) ---
            builder.Services.AddTransient<TeacherFind.Mobile.Features.Profile.Views.ProfilePage>();
            builder.Services.AddTransient<TeacherFind.Mobile.Features.Profile.ViewModels.ProfileViewModel>();

            builder.Services.AddTransient<TeacherFind.Mobile.Features.Profile.Views.ProfileSettingsPage>();
            builder.Services.AddTransient<TeacherFind.Mobile.Features.Profile.ViewModels.ProfileSettingsViewModel>();

            // --- Arama ve Filtreleme Modülü (Search) ---
            builder.Services.AddTransient<TeacherFind.Mobile.Features.Search.Views.SearchFilterPage>();
            builder.Services.AddTransient<TeacherFind.Mobile.Features.Search.ViewModels.SearchFilterViewModel>();

            return builder.Build();
        }
    }
}