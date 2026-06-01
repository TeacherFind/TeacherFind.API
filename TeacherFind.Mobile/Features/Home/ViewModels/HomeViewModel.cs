using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using TeacherFind.Mobile.Core.Abstractions;

namespace TeacherFind.Mobile.Features.Home.ViewModels;

// 1. KATEGORİ UI MODELİ
public class CategoryModel
{
    public string Icon { get; set; }
    public string Title { get; set; }
    public string Subtitle { get; set; }
}

// 2. ÖĞRETMEN KARTI UI MODELİ (Domain'deki User'dan bağımsız hale getirdik)
public class TeacherModel
{
    public string FirstName { get; set; }
    public string ProfilePictureUrl { get; set; }
}

public class HomeViewModel : BindableObject
{
    private readonly IApiService _apiService;
    private bool _isBusy;

    public ObservableCollection<CategoryModel> Categories { get; set; }
    public ObservableCollection<TeacherModel> FeaturedTeachers { get; set; }

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public HomeViewModel(IApiService apiService)
    {
        _apiService = apiService;
        Categories = new ObservableCollection<CategoryModel>();
        FeaturedTeachers = new ObservableCollection<TeacherModel>();

        LoadStaticCategories();
    }

    public async Task InitializeAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            FeaturedTeachers.Clear();

            // Sayfa yüklenme efektini görmek için küçük bir gecikme
            await Task.Delay(1000);

            // Artık kendi TeacherModel'imizi eklediğimiz için User entity'si ile hiçbir çakışma yaşanmaz
            FeaturedTeachers.Add(new TeacherModel
            {
                FirstName = "Emre Koç",
                ProfilePictureUrl = "https://picsum.photos/300/200?random=1"
            });

            FeaturedTeachers.Add(new TeacherModel
            {
                FirstName = "Ayşe Yılmaz",
                ProfilePictureUrl = "https://picsum.photos/300/200?random=2"
            });
        }
        catch (Exception)
        {
            // Hata durumunda liste boş kalır, XAML'deki EmptyView tetiklenir
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void LoadStaticCategories()
    {
        Categories.Add(new CategoryModel { Icon = "📐", Title = "Matematik", Subtitle = "Bilgisayarlı (TYT/AYT)" });
        Categories.Add(new CategoryModel { Icon = "🔬", Title = "Fen Bilimleri", Subtitle = "Fizik/Kimya/Biyoloji" });
        Categories.Add(new CategoryModel { Icon = "💻", Title = "Yazılım & Kodlama", Subtitle = "Mobil/Web" });
        Categories.Add(new CategoryModel { Icon = "🌍", Title = "Dil Kursları", Subtitle = "İngilizce, Almanca" });
    }
}