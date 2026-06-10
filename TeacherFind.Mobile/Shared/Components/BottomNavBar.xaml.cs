using Microsoft.Maui.Controls;
using System;

namespace TeacherFind.Mobile.Shared.Components;

public partial class BottomNavBar : ContentView
{
    public BottomNavBar()
    {
        InitializeComponent();
    }

    private async void OnHomeTapped(object sender, EventArgs e)
    {
        // Go to Home logic (e.g. Navigation.PopToRootAsync or switch detail)
        await AnimateIcon(sender as View);
    }

    private async void OnMessagesTapped(object sender, EventArgs e)
    {
        // Messages logic
        await AnimateIcon(sender as View);
        await Application.Current.MainPage.DisplayAlert("Bilgi", "Mesajlar sayfası henüz yapım aşamasında.", "Tamam");
    }

    private async void OnSearchTapped(object sender, EventArgs e)
    {
        // Big Center Button logic
        await AnimateIcon(sender as View);
        // Navigate to Teacher Search / Discover
        // await Navigation.PushAsync(new TeacherListPage()); vs
        await Application.Current.MainPage.DisplayAlert("Bilgi", "Eğitmen Keşfet sayfası açılacak.", "Tamam");
    }

    private async void OnFavoritesTapped(object sender, EventArgs e)
    {
        // Favorites logic
        await AnimateIcon(sender as View);
        await Application.Current.MainPage.DisplayAlert("Bilgi", "Derslerim / Favoriler sayfası henüz yapım aşamasında.", "Tamam");
    }

    private async void OnProfileTapped(object sender, EventArgs e)
    {
        // Profile logic
        await AnimateIcon(sender as View);
        // Navigate to Profile
        // await Navigation.PushAsync(new ProfilePage());
        await Application.Current.MainPage.DisplayAlert("Bilgi", "Profil sayfası açılacak.", "Tamam");
    }

    private async Task AnimateIcon(View? view)
    {
        if (view == null) return;
        
        await view.ScaleTo(0.8, 100, Easing.CubicOut);
        await view.ScaleTo(1.0, 100, Easing.CubicIn);
    }
}
