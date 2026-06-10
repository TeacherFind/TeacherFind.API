using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TeacherFind.Mobile.Core.Abstractions;
using TeacherFind.Mobile.Core.Models.Location;

namespace TeacherFind.Mobile.Features.Auth.Views;

public partial class RegisterPage : ContentPage
{
    private readonly IApiService _apiService;
    // Steps
    private bool _isStep0 = true;
    public bool IsStep0
    {
        get => _isStep0;
        set { _isStep0 = value; OnPropertyChanged(); }
    }

    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set { _isRefreshing = value; OnPropertyChanged(); }
    }

    private bool _isStep1 = false;
    public bool IsStep1
    {
        get => _isStep1;
        set { _isStep1 = value; OnPropertyChanged(); }
    }

    // Role
    private bool _isTutor = false;
    public bool IsTutor
    {
        get => _isTutor;
        set { _isTutor = value; OnPropertyChanged(); }
    }

    private string _selectedRole = "";

    // Card Colors
    private Color _studentCardBorderColor = Color.FromArgb("#f1f5f9");
    public Color StudentCardBorderColor
    {
        get => _studentCardBorderColor;
        set { _studentCardBorderColor = value; OnPropertyChanged(); }
    }

    private Color _studentCardBackgroundColor = Color.FromArgb("#ffffff");
    public Color StudentCardBackgroundColor
    {
        get => _studentCardBackgroundColor;
        set { _studentCardBackgroundColor = value; OnPropertyChanged(); }
    }

    private Color _tutorCardBorderColor = Color.FromArgb("#f1f5f9");
    public Color TutorCardBorderColor
    {
        get => _tutorCardBorderColor;
        set { _tutorCardBorderColor = value; OnPropertyChanged(); }
    }

    private Color _tutorCardBackgroundColor = Color.FromArgb("#ffffff");
    public Color TutorCardBackgroundColor
    {
        get => _tutorCardBackgroundColor;
        set { _tutorCardBackgroundColor = value; OnPropertyChanged(); }
    }

    // Input Border Colors
    private Color _defaultBorderColor = Color.FromArgb("#e5e7eb");
    public Color DefaultBorderColor
    {
        get => _defaultBorderColor;
        set { _defaultBorderColor = value; OnPropertyChanged(); }
    }

    // Passwords
    private bool _isPasswordHidden = true;
    public bool IsPasswordHidden
    {
        get => _isPasswordHidden;
        set { _isPasswordHidden = value; OnPropertyChanged(); OnPropertyChanged(nameof(PasswordIconGlyph)); }
    }
    public string PasswordIconGlyph => IsPasswordHidden ? "👁" : "✔";

    private bool _isConfirmPasswordHidden = true;
    public bool IsConfirmPasswordHidden
    {
        get => _isConfirmPasswordHidden;
        set { _isConfirmPasswordHidden = value; OnPropertyChanged(); OnPropertyChanged(nameof(ConfirmPasswordIconGlyph)); }
    }
    public string ConfirmPasswordIconGlyph => IsConfirmPasswordHidden ? "👁" : "✔";

    // --- LOCATION API BINDINGS ---
    public ObservableCollection<CityDto> Cities { get; set; } = new();
    public ObservableCollection<DistrictDto> Districts { get; set; } = new();
    public ObservableCollection<NeighborhoodDto> Neighborhoods { get; set; } = new();

    private CityDto? _selectedCity;
    public CityDto? SelectedCity
    {
        get => _selectedCity;
        set 
        { 
            _selectedCity = value; 
            OnPropertyChanged(); 
            if (value != null) 
                _ = LoadDistrictsAsync(value.Id);
            else
                Districts.Clear();
        }
    }

    private DistrictDto? _selectedDistrict;
    public DistrictDto? SelectedDistrict
    {
        get => _selectedDistrict;
        set 
        { 
            _selectedDistrict = value; 
            OnPropertyChanged(); 
            if (value != null) 
                _ = LoadNeighborhoodsAsync(value.Id);
            else
                Neighborhoods.Clear();
        }
    }

    private NeighborhoodDto? _selectedNeighborhood;
    public NeighborhoodDto? SelectedNeighborhood
    {
        get => _selectedNeighborhood;
        set { _selectedNeighborhood = value; OnPropertyChanged(); }
    }

    // Commands
    public ICommand SelectStudentRoleCommand { get; }
    public ICommand SelectTutorRoleCommand { get; }
    public ICommand GoBackToStep0Command { get; }
    public ICommand NavigateToLoginCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand CloseCommand { get; }

    private readonly IServiceProvider _services;

    public RegisterPage(IApiService apiService, IServiceProvider services)
    {
        _apiService = apiService;
        _services = services;
        InitializeComponent();

        if (Application.Current.RequestedTheme == AppTheme.Dark)
        {
            StudentCardBorderColor = Color.FromArgb("#334155");
            StudentCardBackgroundColor = Color.FromArgb("#0f172a");
            TutorCardBorderColor = Color.FromArgb("#334155");
            TutorCardBackgroundColor = Color.FromArgb("#0f172a");
            DefaultBorderColor = Color.FromArgb("#334155");
        }

        SelectStudentRoleCommand = new Command(() => {
            _selectedRole = "student";
            IsTutor = false;
            
            // Set colors
            StudentCardBorderColor = Color.FromArgb("#3b82f6");
            StudentCardBackgroundColor = Application.Current.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#1e3a8a") : Color.FromArgb("#eff6ff");
            
            // Move to step 1
            Task.Delay(300).ContinueWith(_ => {
                Dispatcher.Dispatch(() => {
                    IsStep0 = false;
                    IsStep1 = true;
                });
            });
        });

        SelectTutorRoleCommand = new Command(() => {
            _selectedRole = "tutor";
            IsTutor = true;
            
            // Set colors
            TutorCardBorderColor = Color.FromArgb("#3b82f6");
            TutorCardBackgroundColor = Application.Current.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#1e3a8a") : Color.FromArgb("#eff6ff");
            
            // Move to step 1
            Task.Delay(300).ContinueWith(_ => {
                Dispatcher.Dispatch(() => {
                    IsStep0 = false;
                    IsStep1 = true;
                });
            });
        });

        GoBackToStep0Command = new Command(() => {
            IsStep1 = false;
            IsStep0 = true;
            
            // Reset colors
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
        });

        NavigateToLoginCommand = new Command(async () => {
            // LoginPage is usually in the navigation stack if we came from there,
            // but if we are just switching, we can PopAsync.
            await Navigation.PopAsync();
        });

        RefreshCommand = new Command(async () => {
            IsRefreshing = true;
            await LoadCitiesAsync();
            IsRefreshing = false;
        });

        CloseCommand = new Command(() => {
            if (Application.Current.MainPage is FlyoutPage flyout)
            {
                var homePage = _services.GetService(typeof(TeacherFind.Mobile.Features.Home.Views.HomePage)) as Page;
                flyout.Detail = new NavigationPage(homePage);
            }
        });

        BindingContext = this; // SET AFTER COMMANDS ARE INITIALIZED
        
        // Load cities initially
        _ = LoadCitiesAsync();
    }

    private async Task LoadCitiesAsync()
    {
        try
        {
            var cities = await _apiService.GetAsync<List<CityDto>>("/api/locations/cities");
            if (cities != null)
            {
                Cities.Clear();
                foreach (var city in cities) Cities.Add(city);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Hata", $"Şehirler yüklenemedi: {ex.Message}", "Tamam");
        }
    }

    private async Task LoadDistrictsAsync(Guid cityId)
    {
        try
        {
            var districts = await _apiService.GetAsync<List<DistrictDto>>($"/api/locations/districts?cityId={cityId}");
            Districts.Clear();
            Neighborhoods.Clear();
            if (districts != null)
            {
                foreach (var dist in districts) Districts.Add(dist);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Hata", $"İlçeler yüklenemedi: {ex.Message}", "Tamam");
        }
    }

    private async Task LoadNeighborhoodsAsync(Guid districtId)
    {
        try
        {
            var neighborhoods = await _apiService.GetAsync<List<NeighborhoodDto>>($"/api/locations/neighborhoods?districtId={districtId}");
            Neighborhoods.Clear();
            if (neighborhoods != null)
            {
                foreach (var neigh in neighborhoods) Neighborhoods.Add(neigh);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Hata", $"Mahalleler yüklenemedi: {ex.Message}", "Tamam");
        }
    }

    private void OnCitySelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        if (picker?.SelectedItem is CityDto selectedCity)
        {
            _ = LoadDistrictsAsync(selectedCity.Id);
        }
        else
        {
            Districts.Clear();
            Neighborhoods.Clear();
        }
    }

    private void OnDistrictSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        if (picker?.SelectedItem is DistrictDto selectedDistrict)
        {
            _ = LoadNeighborhoodsAsync(selectedDistrict.Id);
        }
        else
        {
            Neighborhoods.Clear();
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