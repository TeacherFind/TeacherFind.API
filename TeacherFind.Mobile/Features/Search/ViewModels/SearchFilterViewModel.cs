using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TeacherFind.Mobile.Core.Abstractions;
using System.Text.Json.Serialization;

namespace TeacherFind.Mobile.Features.Search.ViewModels;

public class FilterApiCategoryModel
{
    [JsonPropertyName("category")]
    public string CategoryName { get; set; }
}

public class FilterApiCityModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class SearchFilterViewModel : BindableObject
{
    private readonly IApiService _apiService;
    private string _searchText;
    private string _selectedCategory;
    private string _selectedLocation;
    private string _selectedLessonType;
    private string _minPrice;
    private string _maxPrice;
    private bool _isBusy;

    public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }
    public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); } }
    public string SelectedCategory { get => _selectedCategory; set { _selectedCategory = value; OnPropertyChanged(); } }
    public string SelectedLocation { get => _selectedLocation; set { _selectedLocation = value; OnPropertyChanged(); } }
    public string SelectedLessonType { get => _selectedLessonType; set { _selectedLessonType = value; OnPropertyChanged(); } }
    public string MinPrice { get => _minPrice; set { _minPrice = value; OnPropertyChanged(); } }
    public string MaxPrice { get => _maxPrice; set { _maxPrice = value; OnPropertyChanged(); } }

    public ObservableCollection<string> Categories { get; set; } = new();
    public ObservableCollection<string> Locations { get; set; } = new();
    public ObservableCollection<string> LessonTypes { get; set; } = new() { "Tümü", "Online", "Yüz Yüze" };

    public ICommand ApplyFiltersCommand { get; }

    public SearchFilterViewModel(IApiService apiService)
    {
        _apiService = apiService;
        ApplyFiltersCommand = new Command(async () => await ApplyFiltersAsync());
    }

    public async Task InitializeAsync()
    {
        // Eğer zaten veri yüklüyse veya işlem sürüyorsa tekrar tetikleme
        if (IsBusy || (Categories.Count > 1 && Locations.Count > 1)) return;

        try
        {
            IsBusy = true;

            // Kategorileri al
            var apiCategories = await _apiService.GetAsync<List<FilterApiCategoryModel>>("api/categories");
            if (apiCategories != null)
            {
                Categories.Clear();
                Categories.Add("Tüm Kategoriler");
                foreach (var cat in apiCategories.OrderBy(c => c.CategoryName))
                    Categories.Add(cat.CategoryName);
            }

            // Şehirleri al
            var apiCities = await _apiService.GetAsync<List<FilterApiCityModel>>("api/locations/cities");
            if (apiCities != null)
            {
                Locations.Clear();
                Locations.Add("Fark Etmez");
                foreach (var city in apiCities.OrderBy(c => c.Name))
                    Locations.Add(city.Name);
            }
        }
        catch (Exception ex)
        {
            await Microsoft.Maui.Controls.Shell.Current.DisplayAlert("Hata", "Filtre verileri alınamadı.", "Tamam");
            Console.WriteLine($"Hata: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ApplyFiltersAsync()
    {
        // Örnek: Filtre değerlerini kullanarak bir sonraki sayfaya veya API'ye yönlendirme
        await Microsoft.Maui.Controls.Shell.Current.DisplayAlert("Bilgi", $"Arama: {SearchText}, Kategori: {SelectedCategory}", "Tamam");
    }
}