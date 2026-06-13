using System.Collections.ObjectModel;
using TeacherFind.Contracts.Tutors;
using TeacherFind.Mobile.Core.Abstractions;
using TeacherFind.Mobile.Core.Utilities;

namespace TeacherFind.Mobile.Features.Teachers.Views;

public partial class TeacherDetailPage : ContentPage
{
    private readonly IApiService _apiService;

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

    private string _teacherName = string.Empty;
    public string TeacherName { get => _teacherName; set { _teacherName = value; OnPropertyChanged(); } }

    private string _coverPhotoUrl = "default_avatar.png";
    public string CoverPhotoUrl { get => _coverPhotoUrl; set { _coverPhotoUrl = value; OnPropertyChanged(); } }

    private string _title = string.Empty;
    public string TitleText { get => _title; set { _title = value; OnPropertyChanged(); } }

    private string _bio = string.Empty;
    public string Bio { get => _bio; set { _bio = value; OnPropertyChanged(); } }

    private string _price = string.Empty;
    public string Price { get => _price; set { _price = value; OnPropertyChanged(); } }

    private string _rating = string.Empty;
    public string Rating { get => _rating; set { _rating = value; OnPropertyChanged(); } }

    public ObservableCollection<DetailPhotoModel> Photos { get; } = new();

    public TeacherDetailPage(IApiService apiService)
    {
        _apiService = apiService;
        InitializeComponent();
        BindingContext = this;
    }

    public async Task LoadAsync(Guid listingId)
    {
        IsBusy = true;

        try
        {
            var detail = await _apiService.GetAsync<TutorDetailDto>($"api/tutors/{listingId}");

            if (detail is null)
                return;

            TeacherName = DisplayValueHelper.ToPlainText(detail.TeacherName);
            TitleText = DisplayValueHelper.ToPlainText(detail.Title);
            Bio = DisplayValueHelper.ToPlainText(detail.Bio);
            Price = $"{detail.Price:N0} TL/saat";
            Rating = $"⭐ {detail.Rating:0.0} ({detail.ReviewCount})";
            CoverPhotoUrl = DisplayValueHelper.ResolveTutorImageUrl(
                _apiService,
                detail.Photos,
                detail.AvatarUrl);

            Photos.Clear();

            foreach (var photo in detail.Photos.OrderByDescending(x => x.IsMain).ThenBy(x => x.SortOrder))
            {
                Photos.Add(new DetailPhotoModel
                {
                    PhotoUrl = _apiService.ToAbsoluteUrl(photo.PhotoUrl)
                });
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}

public class DetailPhotoModel
{
    public string PhotoUrl { get; set; } = string.Empty;
}
