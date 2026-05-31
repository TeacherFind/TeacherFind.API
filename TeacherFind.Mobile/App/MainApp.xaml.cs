using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace TeacherFind.Mobile;

public partial class MainApp : Application
{
    public MainApp()
    {
        InitializeComponent();

        // ÇÖKMENÝN ASIL SEBEBÝ BURASIYDI: MainPage asla null kalamaz!
        // Þimdilik ekrana test için doðrudan kodla yazýlmýþ bir sayfa veriyoruz.
        MainPage = new ContentPage
        {
            BackgroundColor = Colors.DarkSlateBlue,
            Content = new VerticalStackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = "A-Muallem Motoru Çalýþýyor! ??",
                        TextColor = Colors.White,
                        FontSize = 24,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center
                    }
                }
            }
        };
    }
}