using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TeacherFind.Mobile.Core.Abstractions;
using TeacherFind.Mobile.Core.Models.Location;

namespace TeacherFind.Mobile.Features.Auth.Views;

public partial class RegisterPage : ContentPage
{
    private readonly IApiService _apiService;
    private readonly IServiceProvider _services;
    private bool _isLoadingCities;
    private int _citySelectionVersion;
    private int _districtSelectionVersion;

    // Steps
    private bool _isStep0 = true;
    public bool IsStep0
    {
        get => _isStep0;
        set
        {
            _isStep0 = value;
            OnPropertyChanged();
        }
    }

    private bool _isStep1 = false;
    public bool IsStep1
    {
        get => _isStep1;
        set
        {
            _isStep1 = value;
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

    // Role
    private bool _isTutor = false;
    public bool IsTutor
    {
        get => _isTutor;
        set
        {
            _isTutor = value;
            OnPropertyChanged();
        }
    }

    private string _selectedRole = "";

    // UI States
    private bool _isSubmitting;
    public bool IsSubmitting
    {
        get => _isSubmitting;
        set { 
            _isSubmitting = value; 
            OnPropertyChanged(); 
            OnPropertyChanged(nameof(IsNotSubmitting)); 
        }
    }

    public bool IsNotSubmitting => !IsSubmitting;

    private bool _isPopupVisible;
    public bool IsPopupVisible
    {
        get => _isPopupVisible;
        set { _isPopupVisible = value; OnPropertyChanged(); }
    }

    private string _popupTitle = "";
    public string PopupTitle
    {
        get => _popupTitle;
        set { _popupTitle = value; OnPropertyChanged(); }
    }

    private string _popupMessage = "";
    public string PopupMessage
    {
        get => _popupMessage;
        set { _popupMessage = value; OnPropertyChanged(); }
    }

    private Color _popupColor = Colors.Green;
    public Color PopupColor
    {
        get => _popupColor;
        set { _popupColor = value; OnPropertyChanged(); }
    }

    private string _popupIcon = "✔";
    public string PopupIcon
    {
        get => _popupIcon;
        set { _popupIcon = value; OnPropertyChanged(); }
    }

    private bool _isSuccessPopup;

    public ICommand ClosePopupCommand { get; }

    // Form Fields
    private string _fullName = "";
    public string FullName
    {
        get => _fullName;
        set { _fullName = value; OnPropertyChanged(); }
    }

    private string _email = "";
    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(); }
    }

    private string _phoneNumber = "";
    public string PhoneNumber
    {
        get => _phoneNumber;
        set { _phoneNumber = value; OnPropertyChanged(); }
    }

    private string _password = "";
    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    private string _confirmPassword = "";
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set { _confirmPassword = value; OnPropertyChanged(); }
    }

    private string _gender = "";
    public string Gender
    {
        get => _gender;
        set { _gender = value; OnPropertyChanged(); }
    }

    // Card Colors
    private Color _studentCardBorderColor = Color.FromArgb("#f1f5f9");
    public Color StudentCardBorderColor
    {
        get => _studentCardBorderColor;
        set
        {
            _studentCardBorderColor = value;
            OnPropertyChanged();
        }
    }

    private Color _studentCardBackgroundColor = Color.FromArgb("#ffffff");
    public Color StudentCardBackgroundColor
    {
        get => _studentCardBackgroundColor;
        set
        {
            _studentCardBackgroundColor = value;
            OnPropertyChanged();
        }
    }

    private Color _tutorCardBorderColor = Color.FromArgb("#f1f5f9");
    public Color TutorCardBorderColor
    {
        get => _tutorCardBorderColor;
        set
        {
            _tutorCardBorderColor = value;
            OnPropertyChanged();
        }
    }

    private Color _tutorCardBackgroundColor = Color.FromArgb("#ffffff");
    public Color TutorCardBackgroundColor
    {
        get => _tutorCardBackgroundColor;
        set
        {
            _tutorCardBackgroundColor = value;
            OnPropertyChanged();
        }
    }

    // Input Border Colors
    private Color _defaultBorderColor = Color.FromArgb("#e5e7eb");
    public Color DefaultBorderColor
    {
        get => _defaultBorderColor;
        set
        {
            _defaultBorderColor = value;
            OnPropertyChanged();
        }
    }

    // Passwords
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

    public string PasswordIconGlyph => IsPasswordHidden ? "👁" : "✔";

    private bool _isConfirmPasswordHidden = true;
    public bool IsConfirmPasswordHidden
    {
        get => _isConfirmPasswordHidden;
        set
        {
            _isConfirmPasswordHidden = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ConfirmPasswordIconGlyph));
        }
    }

    public string ConfirmPasswordIconGlyph => IsConfirmPasswordHidden ? "👁" : "✔";

    // Location API Bindings
    public ObservableCollection<CityDto> Cities { get; set; } = new();
    public ObservableCollection<DistrictDto> Districts { get; set; } = new();
    public ObservableCollection<NeighborhoodDto> Neighborhoods { get; set; } = new();

    private CityDto? _selectedCity;
    public CityDto? SelectedCity
    {
        get => _selectedCity;
        set
        {
            if (_selectedCity?.Id == value?.Id)
                return;

            _selectedCity = value;
            OnPropertyChanged();

            _citySelectionVersion++;
            _districtSelectionVersion++;
            Districts.Clear();
            Neighborhoods.Clear();

            _selectedDistrict = null;
            _selectedNeighborhood = null;
            OnPropertyChanged(nameof(SelectedDistrict));
            OnPropertyChanged(nameof(SelectedNeighborhood));

            if (value != null)
                _ = LoadDistrictsAsync(value.Id, _citySelectionVersion);
        }
    }

    private DistrictDto? _selectedDistrict;
    public DistrictDto? SelectedDistrict
    {
        get => _selectedDistrict;
        set
        {
            if (_selectedDistrict?.Id == value?.Id)
                return;

            _selectedDistrict = value;
            OnPropertyChanged();

            _districtSelectionVersion++;
            Neighborhoods.Clear();
            _selectedNeighborhood = null;
            OnPropertyChanged(nameof(SelectedNeighborhood));

            if (value != null)
                _ = LoadNeighborhoodsAsync(value.Id, _districtSelectionVersion);
        }
    }

    private NeighborhoodDto? _selectedNeighborhood;
    public NeighborhoodDto? SelectedNeighborhood
    {
        get => _selectedNeighborhood;
        set
        {
            _selectedNeighborhood = value;
            OnPropertyChanged();
        }
    }

    // Commands
    public ICommand SelectStudentRoleCommand { get; }
    public ICommand SelectTutorRoleCommand { get; }
    public ICommand GoBackToStep0Command { get; }
    public ICommand NavigateToLoginCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand SubmitCommand { get; }

    public RegisterPage(IApiService apiService, IServiceProvider services)
    {
        _apiService = apiService;
        _services = services;

        InitializeComponent();

        ApplyInitialThemeColors();

        SelectStudentRoleCommand = new Command(() =>
        {
            _selectedRole = "student";
            IsTutor = false;

            StudentCardBorderColor = Color.FromArgb("#3b82f6");
            StudentCardBackgroundColor = Application.Current.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#1e3a8a")
                : Color.FromArgb("#eff6ff");

            Task.Delay(300).ContinueWith(_ =>
            {
                Dispatcher.Dispatch(() =>
                {
                    IsStep0 = false;
                    IsStep1 = true;
                });
            });
        });

        SelectTutorRoleCommand = new Command(() =>
        {
            _selectedRole = "tutor";
            IsTutor = true;

            TutorCardBorderColor = Color.FromArgb("#3b82f6");
            TutorCardBackgroundColor = Application.Current.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#1e3a8a")
                : Color.FromArgb("#eff6ff");

            Task.Delay(300).ContinueWith(_ =>
            {
                Dispatcher.Dispatch(() =>
                {
                    IsStep0 = false;
                    IsStep1 = true;
                });
            });
        });

        GoBackToStep0Command = new Command(() =>
        {
            IsStep1 = false;
            IsStep0 = true;
            ResetRoleCardColors();
        });

        NavigateToLoginCommand = new Command(async () =>
        {
            await Navigation.PopAsync();
        });

        RefreshCommand = new Command(async () =>
        {
            IsRefreshing = true;
            await LoadCitiesAsync();
            IsRefreshing = false;
        });

        CloseCommand = new Command(() =>
        {
            if (Application.Current.MainPage is FlyoutPage flyout)
            {
                var homePage = _services.GetService(typeof(TeacherFind.Mobile.Features.Home.Views.HomePage)) as Page;
                flyout.Detail = new NavigationPage(homePage);
            }
        });

        SubmitCommand = new Command(async () => await SubmitRegistrationAsync());
        
        ClosePopupCommand = new Command(async () => 
        {
            IsPopupVisible = false;
            if (_isSuccessPopup) 
            {
                await Navigation.PopAsync(); // Başarılıysa girişe dön
            }
        });

        BindingContext = this;

        _ = LoadCitiesAsync();
    }

    private void ApplyInitialThemeColors()
    {
        if (Application.Current.RequestedTheme == AppTheme.Dark)
        {
            StudentCardBorderColor = Color.FromArgb("#334155");
            StudentCardBackgroundColor = Color.FromArgb("#0f172a");
            TutorCardBorderColor = Color.FromArgb("#334155");
            TutorCardBackgroundColor = Color.FromArgb("#0f172a");
            DefaultBorderColor = Color.FromArgb("#334155");
        }
    }

    private void ResetRoleCardColors()
    {
        if (Application.Current.RequestedTheme == AppTheme.Dark)
        {
            StudentCardBorderColor = Color.FromArgb("#334155");
            StudentCardBackgroundColor = Color.FromArgb("#0f172a");
            TutorCardBorderColor = Color.FromArgb("#334155");
            TutorCardBackgroundColor = Color.FromArgb("#0f172a");
        }
        else
        {
            StudentCardBorderColor = Color.FromArgb("#f1f5f9");
            StudentCardBackgroundColor = Color.FromArgb("#ffffff");
            TutorCardBorderColor = Color.FromArgb("#f1f5f9");
            TutorCardBackgroundColor = Color.FromArgb("#ffffff");
        }
    }

    private async Task LoadCitiesAsync()
    {
        if (_isLoadingCities)
            return;

        _isLoadingCities = true;
        try
        {
            var cities = await _apiService.GetAsync<List<CityDto>>("api/locations/cities");

            Cities.Clear();

            if (cities == null || cities.Count == 0)
                return;

            foreach (var city in cities)
                Cities.Add(city);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Hata",
                $"Şehirler yüklenemedi: {ex.Message}",
                "Tamam");
        }
        finally
        {
            _isLoadingCities = false;
        }
    }

    private async Task LoadDistrictsAsync(Guid cityId, int requestVersion)
    {
        try
        {
            if (cityId == Guid.Empty)
                return;

            var districts = await _apiService.GetAsync<List<DistrictDto>>(
                $"api/locations/districts?cityId={cityId}");

            if (requestVersion != _citySelectionVersion)
                return;

            Districts.Clear();
            Neighborhoods.Clear();

            if (districts == null || districts.Count == 0)
                return;

            foreach (var district in districts)
                Districts.Add(district);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Hata",
                $"İlçeler yüklenemedi: {ex.Message}",
                "Tamam");
        }
    }

    private async Task LoadNeighborhoodsAsync(Guid districtId, int requestVersion)
    {
        try
        {
            if (districtId == Guid.Empty)
                return;

            var neighborhoods = await _apiService.GetAsync<List<NeighborhoodDto>>(
                $"api/locations/neighborhoods?districtId={districtId}");

            if (requestVersion != _districtSelectionVersion)
                return;

            Neighborhoods.Clear();

            if (neighborhoods == null || neighborhoods.Count == 0)
                return;

            foreach (var neighborhood in neighborhoods)
                Neighborhoods.Add(neighborhood);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Hata",
                $"Mahalleler yüklenemedi: {ex.Message}",
                "Tamam");
        }
    }

    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        IsPasswordHidden = !IsPasswordHidden;
    }

    private void OnToggleConfirmPasswordClicked(object sender, EventArgs e)
    {
        IsConfirmPasswordHidden = !IsConfirmPasswordHidden;
    }

    private void ShowPopup(string title, string message, bool isSuccess)
    {
        PopupTitle = title;
        PopupMessage = message;
        PopupColor = isSuccess ? Color.FromArgb("#10b981") : Color.FromArgb("#ef4444"); // Emerald Green : Red
        PopupIcon = isSuccess ? "✔" : "✖";
        _isSuccessPopup = isSuccess;
        IsPopupVisible = true;
    }

    private async Task SubmitRegistrationAsync()
    {
        if (string.IsNullOrWhiteSpace(FullName) || 
            string.IsNullOrWhiteSpace(Email) || 
            string.IsNullOrWhiteSpace(Password))
        {
            ShowPopup("Eksik Bilgi", "Lütfen Ad, E-posta ve Şifre gibi zorunlu alanları doldurun.", false);
            return;
        }

        if (Password != ConfirmPassword)
        {
            ShowPopup("Hata", "Şifreler eşleşmiyor. Lütfen kontrol edin.", false);
            return;
        }

        IsSubmitting = true;

        try
        {
            var request = new
            {
                FullName = FullName,
                Email = Email,
                Password = Password,
                Role = IsTutor ? 1 : 2, // Assuming 1 = Tutor, 2 = Student
                PhoneNumber = PhoneNumber,
                CityId = SelectedCity?.Id,
                DistrictId = SelectedDistrict?.Id,
                NeighborhoodId = SelectedNeighborhood?.Id
            };

            // PostAsync is used for registration. Using object as TResponse or a specific type if known.
            var response = await _apiService.PostAsync<object, object>("api/auth/register", request);
            
            if (response != null)
            {
                ShowPopup("Başarılı", "Kayıt başarıyla oluşturuldu! Şimdi giriş yapabilirsiniz.", true);
            }
            else
            {
                ShowPopup("Kayıt Başarısız", "Kayıt oluşturulamadı. Bilgilerinizi kontrol edip tekrar deneyin.", false);
            }
        }
        catch (Exception ex)
        {
            ShowPopup("Kayıt Hatası", $"Bir hata oluştu: {ex.Message}", false);
        }
        finally
        {
            IsSubmitting = false;
        }
    }
}
