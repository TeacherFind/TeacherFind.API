using System.Windows.Input;
using Microsoft.Maui.Graphics;

namespace TeacherFind.Mobile.Features.Auth.Views;

public partial class LoginPage : ContentPage
{
    // Design properties to mimic React state
    private bool _isPasswordHidden = true;
    public bool IsPasswordHidden
    {
        get => _isPasswordHidden;
        set
        {
            _isPasswordHidden = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PasswordIconGlyph));
        }
    }

    // Using Unicode characters for simple icons since we didn't import a font yet
    // "👁" vs "👁‍🗨" or similar. For simplicity, we just use a generic glyph placeholder.
    public string PasswordIconGlyph => IsPasswordHidden ? "👁" : "✔";

    private Color _emailBorderColor = Color.FromArgb("#e5e7eb"); // Default BorderLightColor
    public Color EmailBorderColor
    {
        get => _emailBorderColor;
        set
        {
            _emailBorderColor = value;
            OnPropertyChanged();
        }
    }

    private Color _passwordBorderColor = Color.FromArgb("#e5e7eb");
    public Color PasswordBorderColor
    {
        get => _passwordBorderColor;
        set
        {
            _passwordBorderColor = value;
            OnPropertyChanged();
        }
    }

    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoginCommand { get; }
    public ICommand NavigateToRegisterCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand CloseCommand { get; }
    private readonly IServiceProvider _services;

    public LoginPage(IServiceProvider services)
    {
        _services = services;
        InitializeComponent();
        
        // Ensure dark mode initial colors
        if (Application.Current.RequestedTheme == AppTheme.Dark)
        {
            EmailBorderColor = Color.FromArgb("#334155");
            PasswordBorderColor = Color.FromArgb("#334155");
        }

        NavigateToRegisterCommand = new Command(async () => {
            try 
            {
                if (Navigation == null) 
                {
                    await Application.Current.MainPage.DisplayAlert("Hata", "Navigation nesnesi null!", "Tamam");
                    return;
                }
                var registerPage = (RegisterPage)_services.GetService(typeof(RegisterPage));
                await Navigation.PushAsync(registerPage);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Hata", ex.Message, "Tamam");
            }
        });

        RefreshCommand = new Command(async () => {
            IsRefreshing = true;
            
            // Re-evaluate themes or reset fields if needed. Here we just mock a refresh.
            await Task.Delay(1000); 

            IsRefreshing = false;
        });

        CloseCommand = new Command(() => {
            // Uygulamanın ana iskeletini (FlyoutPage) alıp, içeriğini Ana Sayfa'ya çeviriyoruz:
            if (Application.Current.MainPage is FlyoutPage flyout)
            {
                var homePage = _services.GetService(typeof(TeacherFind.Mobile.Features.Home.Views.HomePage)) as Page;
                flyout.Detail = new NavigationPage(homePage);
            }
        });

        BindingContext = this; // SET AFTER COMMANDS ARE INITIALIZED
    }

    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        IsPasswordHidden = !IsPasswordHidden;
    }

    private void OnEntryFocused(object sender, FocusEventArgs e)
    {
        var entry = sender as Entry;
        if (entry == null) return;

        // Apply primary blue color on focus
        var focusColor = Color.FromArgb("#2d79f3");

        if (entry.Placeholder == "E-posta adresinizi girin")
        {
            EmailBorderColor = focusColor;
        }
        else if (entry.Placeholder == "Şifrenizi girin")
        {
            PasswordBorderColor = focusColor;
        }
    }

    private void OnEntryUnfocused(object sender, FocusEventArgs e)
    {
        var entry = sender as Entry;
        if (entry == null) return;

        // Revert to normal border color based on theme
        var defaultColor = Application.Current.RequestedTheme == AppTheme.Dark 
            ? Color.FromArgb("#334155") 
            : Color.FromArgb("#e5e7eb");

        if (entry.Placeholder == "E-posta adresinizi girin")
        {
            EmailBorderColor = defaultColor;
        }
        else if (entry.Placeholder == "Şifrenizi girin")
        {
            PasswordBorderColor = defaultColor;
        }
    }
}
