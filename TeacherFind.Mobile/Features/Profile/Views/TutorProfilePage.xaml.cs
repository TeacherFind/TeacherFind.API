using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Media;
using TeacherFind.Contracts.Education;
using TeacherFind.Contracts.Tutors;
using TeacherFind.Mobile.Core.Abstractions;

namespace TeacherFind.Mobile.Features.Profile.Views;

public partial class TutorProfilePage : ContentPage
{
    private readonly IApiService _apiService;
    private Guid? _loadedDepartmentsUniversityId;
    private bool _isApplyingEducationSelection;

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    private string _fullName = string.Empty;
    public string FullName { get => _fullName; set { _fullName = value; OnPropertyChanged(); } }

    private string _email = string.Empty;
    public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

    private string _phone = string.Empty;
    public string Phone { get => _phone; set { _phone = value; OnPropertyChanged(); } }

    private string _headline = string.Empty;
    public string Headline { get => _headline; set { _headline = value; OnPropertyChanged(); } }

    private string _bio = string.Empty;
    public string Bio { get => _bio; set { _bio = value; OnPropertyChanged(); } }

    private string _teachingStyle = string.Empty;
    public string TeachingStyle { get => _teachingStyle; set { _teachingStyle = value; OnPropertyChanged(); } }

    private string _experience = string.Empty;
    public string Experience { get => _experience; set { _experience = value; OnPropertyChanged(); } }

    private string _profileImageUrl = "default_avatar.png";
    public string ProfileImageUrl { get => _profileImageUrl; set { _profileImageUrl = value; OnPropertyChanged(); } }

    public ObservableCollection<UniversityDto> Universities { get; } = new();
    public ObservableCollection<DepartmentDto> Departments { get; } = new();

    private UniversityDto? _selectedUniversity;
    public UniversityDto? SelectedUniversity
    {
        get => _selectedUniversity;
        set
        {
            if (_selectedUniversity?.Id == value?.Id)
                return;

            _selectedUniversity = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsDepartmentEnabled));

            Departments.Clear();
            _loadedDepartmentsUniversityId = null;
            SelectedDepartment = null;

            if (value is not null && !_isApplyingEducationSelection)
                _ = LoadDepartmentsAsync(value.Id);
        }
    }

    private DepartmentDto? _selectedDepartment;
    public DepartmentDto? SelectedDepartment
    {
        get => _selectedDepartment;
        set
        {
            if (_selectedDepartment?.Id == value?.Id)
                return;

            _selectedDepartment = value;
            OnPropertyChanged();
        }
    }

    public bool IsDepartmentEnabled => SelectedUniversity is not null;

    public ICommand SaveCommand { get; }
    public ICommand UploadAvatarCommand { get; }

    public TutorProfilePage(IApiService apiService)
    {
        _apiService = apiService;
        InitializeComponent();

        SaveCommand = new Command(async () => await SaveProfileAsync());
        UploadAvatarCommand = new Command(async () => await UploadAvatarActionAsync());

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        IsBusy = true;
        try
        {
            await LoadUniversitiesAsync();
            await LoadProfileAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadProfileAsync()
    {
        try
        {
            var profile = await _apiService.GetAsync<TutorProfileDto>("api/tutors/profile");
            if (profile is null)
            {
                await DisplayAlert("Hata", "Profil bilgileri alınamadı.", "Tamam");
                return;
            }

            FullName = profile.FullName ?? string.Empty;
            Email = profile.Email ?? string.Empty;
            Phone = profile.PhoneNumber ?? string.Empty;
            Headline = profile.Headline ?? profile.Title ?? string.Empty;
            Bio = profile.Bio ?? string.Empty;
            TeachingStyle = profile.TeachingStyle ?? string.Empty;
            Experience = profile.EducationLevel ?? string.Empty;
            ProfileImageUrl = _apiService.ToAbsoluteUrl(profile.AvatarUrl ?? profile.ProfileImageUrl);

            await ApplyEducationSelectionAsync(profile.UniversityId, profile.DepartmentId);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", "Profil yüklenemedi: " + ex.Message, "Tamam");
        }
    }

    private async Task LoadUniversitiesAsync()
    {
        if (Universities.Count > 0)
            return;

        var universities = await _apiService.GetAsync<List<UniversityDto>>("api/education/universities");
        Universities.Clear();

        if (universities is null || universities.Count == 0)
            return;

        foreach (var university in universities)
            Universities.Add(university);
    }

    private async Task LoadDepartmentsAsync(Guid universityId)
    {
        if (universityId == Guid.Empty)
            return;

        if (_loadedDepartmentsUniversityId == universityId && Departments.Count > 0)
            return;

        var departments = await _apiService.GetAsync<List<DepartmentDto>>(
            $"api/education/departments?universityId={universityId}");

        Departments.Clear();
        _loadedDepartmentsUniversityId = universityId;

        if (departments is null || departments.Count == 0)
            return;

        foreach (var department in departments)
            Departments.Add(department);
    }

    private async Task ApplyEducationSelectionAsync(Guid? universityId, Guid? departmentId)
    {
        _isApplyingEducationSelection = true;
        try
        {
            SelectedUniversity = universityId.HasValue
                ? Universities.FirstOrDefault(x => x.Id == universityId.Value)
                : null;
        }
        finally
        {
            _isApplyingEducationSelection = false;
        }

        if (SelectedUniversity is null)
            return;

        await LoadDepartmentsAsync(SelectedUniversity.Id);

        SelectedDepartment = departmentId.HasValue
            ? Departments.FirstOrDefault(x => x.Id == departmentId.Value)
            : null;
    }

    private async Task SaveProfileAsync()
    {
        IsBusy = true;
        try
        {
            var request = new UpdateTutorProfileDto
            {
                Headline = Headline,
                Bio = Bio,
                TeachingStyle = TeachingStyle,
                UniversityId = SelectedUniversity?.Id,
                DepartmentId = SelectedDepartment?.Id
            };

            var isSaved = await _apiService.PutAsync("api/tutors/profile", request);
            await DisplayAlert(
                isSaved ? "Başarılı" : "Hata",
                isSaved ? "Profiliniz başarıyla güncellendi." : "Profil kaydedilemedi.",
                "Tamam");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", "Kaydedilirken bir hata oluştu: " + ex.Message, "Tamam");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UploadAvatarActionAsync()
    {
        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Profil resmi seçin"
            });

            if (result is null)
                return;

            IsBusy = true;
            var response = await _apiService.UploadFileAsync<AvatarUploadResponse>(
                "api/tutors/avatar",
                result);

            if (response?.ProfileImageUrl is null)
            {
                await DisplayAlert("Hata", "Profil resmi yüklenemedi.", "Tamam");
                return;
            }

            ProfileImageUrl = _apiService.ToAbsoluteUrl(response.ProfileImageUrl);
            await DisplayAlert("Başarılı", "Profil resmi güncellendi.", "Tamam");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", "Resim yüklenemedi: " + ex.Message, "Tamam");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private sealed class AvatarUploadResponse
    {
        public string? ProfileImageUrl { get; set; }
    }
}
