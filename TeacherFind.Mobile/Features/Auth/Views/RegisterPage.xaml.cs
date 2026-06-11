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
}
