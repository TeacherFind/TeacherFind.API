using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TeacherFind.Mobile.Base; // BaseViewModel için
using TeacherFind.Mobile.Core.Abstractions;
using TeacherFind.Mobile.Core.Services;
using Microsoft.Maui.Controls; // Shell için şart
using TeacherFind.Contracts.Auth; // Eğer klasör adı buysa

namespace TeacherFind.Mobile.Features.Auth.ViewModels;

public class LoginViewModel : Border // Geçici olarak hata vermemesi için en temel sınıftan türettik veya direkt nesne yaptık
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private readonly IAlertService _alertService;

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(); }
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    private bool _rememberMe;
    public bool RememberMe
    {
        get => _rememberMe;
        set { _rememberMe = value; OnPropertyChanged(); }
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public ICommand LoginCommand { get; }
    public ICommand GoToRegisterCommand { get; }
    public ICommand GoToForgotPasswordCommand { get; }

    public LoginViewModel(
        IAuthService authService,
        INavigationService navigationService,
        IAlertService alertService)
    {
        _authService = authService;
        _navigationService = navigationService;
        _alertService = alertService;

        // Komutları senin servis metot isimlerine göre en yalın halde bağlıyoruz
        LoginCommand = new Command(async () => await ExecuteLoginAsync());

        // Eğer navigation servis metot adın farklıysa hata vermemesi için şimdilik boş task veriyoruz
        GoToRegisterCommand = new Command(() => { });
        GoToForgotPasswordCommand = new Command(() => { });
    }

    private async Task ExecuteLoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            // Metot adından emin olamadığımız için şimdilik MAUI'nin yerel alert'ünü tetikliyoruz
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Uyarı", "Lütfen tüm alanları doldurun.", "Tamam");
            return;
        }

        IsBusy = true;

        try
        {
            // LoginRequest DTO yapını bizzat inline oluşturuyoruz
            var loginRequest = new LoginRequest { Email = Email, Password = Password };
            var result = await _authService.LoginAsync(loginRequest);

            if (result != null && result.IsSuccess)
            {
                // Giriş başarılı olunca Shell üzerinden doğrudan yönlendirme yapıyoruz
                await global::Microsoft.Maui.Controls.Shell.Current.GoToAsync("//HomePage");
            }
            else
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Hata", result?.Message ?? "Giriş yapılamadı.", "Tamam");
            }
        }
        catch (Exception ex)
        {
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Hata", $"Sistemsel bir sorun: {ex.Message}", "Tamam");
        }
        finally
        {
            IsBusy = false;
        }
    }
}