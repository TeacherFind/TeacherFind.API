using System.Windows.Input;
using System.Net.Mail;
using Microsoft.Maui.Media;
using TeacherFind.Contracts.Students;
using TeacherFind.Mobile.Core.Abstractions;

namespace TeacherFind.Mobile.Features.Profile.Views;

public partial class StudentProfilePage : ContentPage
{
    private readonly IApiService _apiService;

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

    private string _bio = string.Empty;
    public string Bio { get => _bio; set { _bio = value; OnPropertyChanged(); } }

    private string _profileImageUrl = "default_avatar.png";
    public string ProfileImageUrl { get => _profileImageUrl; set { _profileImageUrl = value; OnPropertyChanged(); } }

    private bool _isEmailChangeModalVisible;
    public bool IsEmailChangeModalVisible
    {
        get => _isEmailChangeModalVisible;
        set
        {
            _isEmailChangeModalVisible = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsEmailChangeRequestStepVisible));
        }
    }

    private bool _isEmailChangeCodeStepVisible;
    public bool IsEmailChangeCodeStepVisible
    {
        get => _isEmailChangeCodeStepVisible;
        set
        {
            _isEmailChangeCodeStepVisible = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsEmailChangeRequestStepVisible));
        }
    }

    public bool IsEmailChangeRequestStepVisible => IsEmailChangeModalVisible && !IsEmailChangeCodeStepVisible;

    private string _newEmail = string.Empty;
    public string NewEmail { get => _newEmail; set { _newEmail = value; OnPropertyChanged(); } }

    private string _emailChangeCode = string.Empty;
    public string EmailChangeCode { get => _emailChangeCode; set { _emailChangeCode = value; OnPropertyChanged(); } }

    private string _emailChangeMessage = string.Empty;
    public string EmailChangeMessage { get => _emailChangeMessage; set { _emailChangeMessage = value; OnPropertyChanged(); } }

    public ICommand SaveCommand { get; }
    public ICommand UploadAvatarCommand { get; }
    public ICommand OpenEmailChangeCommand { get; }
    public ICommand CloseEmailChangeCommand { get; }
    public ICommand SendEmailChangeCodeCommand { get; }
    public ICommand VerifyEmailChangeCommand { get; }

    public StudentProfilePage(IApiService apiService)
    {
        _apiService = apiService;
        InitializeComponent();

        SaveCommand = new Command(async () => await SaveProfileAsync());
        UploadAvatarCommand = new Command(async () => await UploadAvatarActionAsync());
        OpenEmailChangeCommand = new Command(OpenEmailChangeModal);
        CloseEmailChangeCommand = new Command(CloseEmailChangeModal);
        SendEmailChangeCodeCommand = new Command(async () => await SendEmailChangeCodeAsync());
        VerifyEmailChangeCommand = new Command(async () => await VerifyEmailChangeAsync());

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
            var profile = await _apiService.GetAsync<StudentProfileDto>("api/students/profile");
            if (profile is null)
            {
                await DisplayAlert("Hata", "Profil bilgileri alınamadı.", "Tamam");
                return;
            }

            FullName = profile.FullName ?? string.Empty;
            Email = profile.Email ?? string.Empty;
            Phone = profile.PhoneNumber ?? string.Empty;
            Bio = profile.Bio ?? string.Empty;
            ProfileImageUrl = _apiService.ToAbsoluteUrl(profile.ProfileImageUrl);
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
            var request = new UpdateStudentProfileDto
            {
                FullName = FullName,
                PhoneNumber = Phone,
                Bio = Bio
            };

            var isSaved = await _apiService.PutAsync("api/students/profile", request);
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
                "api/students/avatar",
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

    private void OpenEmailChangeModal()
    {
        NewEmail = string.Empty;
        EmailChangeCode = string.Empty;
        EmailChangeMessage = string.Empty;
        IsEmailChangeCodeStepVisible = false;
        IsEmailChangeModalVisible = true;
    }

    private void CloseEmailChangeModal()
    {
        IsEmailChangeModalVisible = false;
        IsEmailChangeCodeStepVisible = false;
        EmailChangeMessage = string.Empty;
    }

    private async Task SendEmailChangeCodeAsync()
    {
        if (!IsValidEmail(NewEmail))
        {
            EmailChangeMessage = "Geçerli bir e-posta adresi girin.";
            return;
        }

        IsBusy = true;

        try
        {
            var response = await _apiService.PostAsync<object, ApiMessageResponse>(
                "api/auth/request-email-change",
                new { newEmail = NewEmail.Trim() });

            EmailChangeMessage = response?.Message ?? "Doğrulama kodu gönderildi.";
            IsEmailChangeCodeStepVisible = true;
        }
        catch (Exception ex)
        {
            EmailChangeMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task VerifyEmailChangeAsync()
    {
        if (!IsValidEmail(NewEmail) || string.IsNullOrWhiteSpace(EmailChangeCode))
        {
            EmailChangeMessage = "Yeni e-posta ve doğrulama kodu zorunludur.";
            return;
        }

        IsBusy = true;

        try
        {
            var response = await _apiService.PostAsync<object, ApiMessageResponse>(
                "api/auth/verify-email-change",
                new
                {
                    newEmail = NewEmail.Trim(),
                    code = EmailChangeCode.Trim()
                });

            CloseEmailChangeModal();
            await LoadProfileAsync();
            await DisplayAlert("Başarılı", response?.Message ?? "E-posta adresiniz güncellendi.", "Tamam");
        }
        catch (Exception ex)
        {
            EmailChangeMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            _ = new MailAddress(email.Trim());
            return true;
        }
        catch
        {
            return false;
        }
    }

    private sealed class AvatarUploadResponse
    {
        public string? ProfileImageUrl { get; set; }
    }

    private sealed class ApiMessageResponse
    {
        public string? Message { get; set; }
    }
}
