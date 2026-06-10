using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TeacherFind.Mobile.Core.Abstractions;

namespace TeacherFind.Mobile.Features.Profile.Views;

public partial class StudentProfilePage : ContentPage
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

    private string _bio;
    public string Bio { get => _bio; set { _bio = value; OnPropertyChanged(); } }

    private string _profileImageUrl = "default_avatar.png"; 
    public string ProfileImageUrl { get => _profileImageUrl; set { _profileImageUrl = value; OnPropertyChanged(); } }

    // Commands
    public ICommand SaveCommand { get; }
    public ICommand UploadAvatarCommand { get; }

    public StudentProfilePage(IApiService apiService)
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
    }

    private async Task LoadProfileAsync()
    {
        IsBusy = true;
        try
        {
            await Task.Delay(500); 

            FullName = "Ferit (Öğrenci)";
            Email = "ferit_student@example.com";
            Phone = "05551234567";
            Bio = "Matematik çalışmayı severim.";
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

    private async Task SaveProfileAsync()
    {
        IsBusy = true;
        try
        {
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
                await Task.Delay(1000);
                ProfileImageUrl = result.FullPath;
                IsBusy = false;
                await DisplayAlert("Başarılı", "Profil resmi seçildi.", "Tamam");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", "Resim seçilemedi.", "Tamam");
        }
    }
}
