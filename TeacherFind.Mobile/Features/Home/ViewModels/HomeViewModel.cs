using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using TeacherFind.Contracts.Common;
using TeacherFind.Contracts.Tutors;
using TeacherFind.Mobile.Core.Abstractions;
using TeacherFind.Mobile.Core.Utilities;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace TeacherFind.Mobile.Features.Home.ViewModels;

// --- API MODELLERİ (BACKEND İLE HABERLEŞEN KISIM) ---
public class CityModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("plateCode")]
    public int PlateCode { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class SubjectModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("code")]
    public int Code { get; set; }
    [JsonPropertyName("stage")]
    public string Stage { get; set; }
    [JsonPropertyName("category")]
    public string Category { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("level")]
    public string Level { get; set; }
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    public string DisplayName => $"{Name} - {Level}";
}

// Kategorinin içindeki dersleri sayabilmek için gelen kısa model
public class ApiSubjectShortModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

// Backend'den Gelen Kategori Modeli
public class ApiCategoryModel
{
    [JsonPropertyName("category")]
    public string CategoryName { get; set; }
    [JsonPropertyName("subjects")]
    public List<ApiSubjectShortModel> Subjects { get; set; }
}

// --- ARAYÜZ (UI) MODELLERİ (EKRANDA GÖSTERİLEN KISIM) ---
public class CategoryModel
{
    public string Icon { get; set; }
    public string Title { get; set; }
    public string Subtitle { get; set; }
}

public class TeacherModel
{
    public string FirstName { get; set; }
    public string ProfilePictureUrl { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Price { get; set; }
    public double Rating { get; set; }
}

public class HomeViewModel : BindableObject
{
    private readonly IApiService _apiService;
    private bool _isBusy;
    private bool _isRefreshing;
    private CityModel _selectedCity;
    private SubjectModel _selectedSubject;

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set { _isRefreshing = value; OnPropertyChanged(); }
    }

    public ObservableCollection<CityModel> Cities { get; set; }
    public ObservableCollection<SubjectModel> Subjects { get; set; }
    public ObservableCollection<CategoryModel> Categories { get; set; }
    public ObservableCollection<TeacherModel> FeaturedTeachers { get; set; }

    public CityModel SelectedCity
    {
        get => _selectedCity;
        set { _selectedCity = value; OnPropertyChanged(); }
    }

    public SubjectModel SelectedSubject
    {
        get => _selectedSubject;
        set { _selectedSubject = value; OnPropertyChanged(); }
    }

    public ICommand RefreshCommand { get; }

    public HomeViewModel(IApiService apiService)
    {
        _apiService = apiService;
        Cities = new ObservableCollection<CityModel>();
        Subjects = new ObservableCollection<SubjectModel>();
        Categories = new ObservableCollection<CategoryModel>();
        FeaturedTeachers = new ObservableCollection<TeacherModel>();

        RefreshCommand = new Command(async () => await RefreshDataAsync());
    }

    private async Task RefreshDataAsync()
    {
        IsRefreshing = true;
        await InitializeAsync(forceRefresh: true);
        IsRefreshing = false;
    }

    public async Task InitializeAsync(bool forceRefresh = false)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            // 1. ŞEHİRLERİ ÇEK
            if (forceRefresh || Cities.Count == 0)
            {
                var apiCities = await _apiService.GetAsync<List<CityModel>>("api/locations/cities");
                if (apiCities != null && apiCities.Any())
                {
                    Cities.Clear();
                    foreach (var city in apiCities.OrderBy(c => c.Name))
                        Cities.Add(city);
                }
            }

            // 2. DERSLERİ ÇEK
            if (forceRefresh || Subjects.Count == 0)
            {
                var apiSubjects = await _apiService.GetAsync<List<SubjectModel>>("api/subjects");
                if (apiSubjects != null && apiSubjects.Any())
                {
                    Subjects.Clear();
                    foreach (var subject in apiSubjects.Where(s => s.IsActive).OrderBy(s => s.Name))
                        Subjects.Add(subject);
                }
            }

            // 3. KATEGORİLERİ ÇEK (YENİ EKLENEN KISIM)
            if (forceRefresh || Categories.Count == 0)
            {
                var apiCategories = await _apiService.GetAsync<List<ApiCategoryModel>>("api/categories");
                if (apiCategories != null && apiCategories.Any())
                {
                    Categories.Clear();

                    // İçinde en çok ders olan kategori en başa gelsin (En Popüler Mantığı)
                    var sortedCategories = apiCategories.OrderByDescending(c => c.Subjects?.Count ?? 0).ToList();

                    foreach (var apiCat in sortedCategories)
                    {
                        Categories.Add(new CategoryModel
                        {
                            Title = apiCat.CategoryName,
                            // Kaç ders varsa alt başlığa onu yazdırıyoruz
                            Subtitle = $"{(apiCat.Subjects?.Count ?? 0)} Farklı Ders",
                            // Otomatik ikon seçiciyi çağırıyoruz
                            Icon = GetIconForCategory(apiCat.CategoryName)
                        });
                    }
                }
            }

            if (forceRefresh || FeaturedTeachers.Count == 0)
            {
                var featured = await _apiService.GetAsync<PagedResultDto<TutorListItemDto>>(
                    "api/tutors?page=1&pageSize=10&sort=rating");

                if (featured?.Items is not null)
                {
                    FeaturedTeachers.Clear();

                    foreach (var tutor in featured.Items)
                    {
                        FeaturedTeachers.Add(new TeacherModel
                        {
                            FirstName = DisplayValueHelper.ToPlainText(tutor.TeacherName),
                            ProfilePictureUrl = DisplayValueHelper.ResolveTutorImageUrl(
                                _apiService,
                                tutor.Photos),
                            Title = DisplayValueHelper.ToPlainText(tutor.Title),
                            Description = DisplayValueHelper.TruncatePlainText(tutor.Description, 90),
                            Price = $"{tutor.Price:N0} TL/saat",
                            Rating = tutor.Rating
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ana Sayfa Veri Çekme Hatası: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Kategori ismine göre mantıklı ikon atayan yardımcı metot
    private string GetIconForCategory(string categoryName)
    {
        if (string.IsNullOrEmpty(categoryName)) return "📚";

        var lower = categoryName.ToLower();
        if (lower.Contains("matematik") || lower.Contains("geometri")) return "📐";
        if (lower.Contains("fen") || lower.Contains("kimya") || lower.Contains("biyoloji") || lower.Contains("fizik")) return "🔬";
        if (lower.Contains("dil") || lower.Contains("ingilizce") || lower.Contains("almanca")) return "🌍";
        if (lower.Contains("yazılım") || lower.Contains("bilişim") || lower.Contains("bilgisayar")) return "💻";
        if (lower.Contains("sanat") || lower.Contains("müzik") || lower.Contains("resim")) return "🎨";
        if (lower.Contains("spor") || lower.Contains("beden")) return "🏃‍♂️";
        if (lower.Contains("tarih") || lower.Contains("coğrafya") || lower.Contains("sosyal")) return "🏛️";
        if (lower.Contains("sınav") || lower.Contains("lgs") || lower.Contains("yks")) return "🎯";

        return "📘"; // Eğer kelimeyi tanımazsa standart kitap ikonu koysun
    }
}
