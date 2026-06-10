using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TeacherFind.Mobile.Core.Abstractions;

namespace TeacherFind.Mobile.Features.Profile.Views;

public partial class TutorProfilePage : ContentPage
{
    private readonly IApiService _apiService;

    // Loading State
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    // Properties
    private string _fullName;
    public string FullName { get => _fullName; set { _fullName = value; OnPropertyChanged(); } }

    private string _email;
    public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

    private string _phone;
    public string Phone { get => _phone; set { _phone = value; OnPropertyChanged(); } }

    private string _headline;
    public string Headline { get => _headline; set { _headline = value; OnPropertyChanged(); } }

    private string _bio;
    public string Bio { get => _bio; set { _bio = value; OnPropertyChanged(); } }

    private string _teachingStyle;
    public string TeachingStyle { get => _teachingStyle; set { _teachingStyle = value; OnPropertyChanged(); } }

    private string _experience;
    public string Experience { get => _experience; set { _experience = value; OnPropertyChanged(); } }

    private string _profileImageUrl = "default_avatar.png"; // Placeholder
    public string ProfileImageUrl { get => _profileImageUrl; set { _profileImageUrl = value; OnPropertyChanged(); } }

    // Dropdown Data
    public ObservableCollection<UniversityModel> Universities { get; } = new();
    public ObservableCollection<DepartmentModel> Departments { get; } = new();

    private UniversityModel _selectedUniversity;
    public UniversityModel SelectedUniversity
    {
        get => _selectedUniversity;
        set
        {
            _selectedUniversity = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsDepartmentEnabled));
            if (value != null)
                _ = LoadDepartmentsAsync(value.Id);
            else
                Departments.Clear();
        }
    }

    private DepartmentModel _selectedDepartment;
    public DepartmentModel SelectedDepartment { get => _selectedDepartment; set { _selectedDepartment = value; OnPropertyChanged(); } }

    public bool IsDepartmentEnabled => SelectedUniversity != null;

    // Commands
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
        await LoadProfileAsync();
        await LoadUniversitiesAsync();
    }

    private async Task LoadProfileAsync()
    {
        IsBusy = true;
        try
        {
            // Placeholder: await _apiService.GetAsync<MyProfileModel>("api/tutors/my-profile");
            await Task.Delay(500); // Simulate network

            // Mock Data
            FullName = "Ferit";
            Email = "ferit@example.com";
            Phone = "05554443322";
            Headline = "Deneyimli Matematik Eğitmeni";
            Bio = "10 yıllık özel ders tecrübem var.";
            TeachingStyle = "Bol soru çözümü.";
            Experience = "Çeşitli kurumlarda çalıştım.";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", "Profil yüklenemedi: " + ex.Message, "Tamam");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadUniversitiesAsync()
    {
        // Placeholder
        Universities.Add(new UniversityModel { Id = 1, Name = "İstanbul Üniversitesi" });
        Universities.Add(new UniversityModel { Id = 2, Name = "Ankara Üniversitesi" });
    }

    private async Task LoadDepartmentsAsync(int universityId)
    {
        Departments.Clear();
        // Placeholder
        Departments.Add(new DepartmentModel { Id = 101, Name = "Bilgisayar Mühendisliği" });
        Departments.Add(new DepartmentModel { Id = 102, Name = "Matematik Öğretmenliği" });
    }

    private async Task SaveProfileAsync()
    {
        IsBusy = true;
        try
        {
            // Placeholder: Save logic
            await Task.Delay(1000);
            await DisplayAlert("Başarılı", "Profiliniz başarıyla güncellendi!", "Tamam");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", "Kaydedilirken bir hata oluştu.", "Tamam");
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
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Profil Resmi Seçin"
            });

            if (result != null)
            {
                IsBusy = true;
                // Upload logic placeholder
                await Task.Delay(1000);
                ProfileImageUrl = result.FullPath;
                IsBusy = false;
                await DisplayAlert("Başarılı", "Profil resmi seçildi (Simülasyon).", "Tamam");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", "Resim seçilemedi.", "Tamam");
        }
    }
}

public class UniversityModel
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class DepartmentModel
{
    public int Id { get; set; }
    public string Name { get; set; }
}
