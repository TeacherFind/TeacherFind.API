using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace TeacherFind.Mobile.Features.Search.ViewModels;

public class SearchFilterViewModel : BindableObject
{
    private string _searchText;
    private string _selectedCategory;
    private string _selectedLocation;
    private string _selectedLessonType;
    private string _minPrice;
    private string _maxPrice;

    public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); } }
    public string SelectedCategory { get => _selectedCategory; set { _selectedCategory = value; OnPropertyChanged(); } }
    public string SelectedLocation { get => _selectedLocation; set { _selectedLocation = value; OnPropertyChanged(); } }
    public string SelectedLessonType { get => _selectedLessonType; set { _selectedLessonType = value; OnPropertyChanged(); } }
    public string MinPrice { get => _minPrice; set { _minPrice = value; OnPropertyChanged(); } }
    public string MaxPrice { get => _maxPrice; set { _maxPrice = value; OnPropertyChanged(); } }

    public ObservableCollection<string> Categories { get; set; }
    public ObservableCollection<string> Locations { get; set; }
    public ObservableCollection<string> LessonTypes { get; set; }

    public ICommand ApplyFiltersCommand { get; }

    public SearchFilterViewModel()
    {
        Categories = new ObservableCollection<string> { "Tüm Kategoriler", "Matematik", "Tango", "Osmanlı Türkçesi", "İngilizce" };
        Locations = new ObservableCollection<string> { "Fark Etmez", "Ankara", "Adıyaman", "Afyonkarahisar" };
        LessonTypes = new ObservableCollection<string> { "Tümü", "Çevrimiçi", "Yüz Yüze" };

        SelectedCategory = Categories[0];
        SelectedLocation = Locations[0];
        SelectedLessonType = LessonTypes[0];

        ApplyFiltersCommand = new Command(async () => await ApplyFiltersAsync());
    }

    private async Task ApplyFiltersAsync()
    {
        // Filtreleri uygulayıp ana listeye döner
        await Application.Current.MainPage.Navigation.PopModalAsync();
    }
}