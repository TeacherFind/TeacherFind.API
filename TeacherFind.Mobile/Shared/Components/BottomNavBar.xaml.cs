using TeacherFind.Mobile.Core.Navigation;

namespace TeacherFind.Mobile.Shared.Components;

public partial class BottomNavBar : ContentView
{
    public BottomNavBar()
    {
        InitializeComponent();
    }

    private async void OnHomeTapped(object sender, EventArgs e)
    {
        await AnimateIcon(sender as View);
    }

    private async void OnMessagesTapped(object sender, EventArgs e)
    {
        await AnimateIcon(sender as View);
        await Application.Current.MainPage.DisplayAlert("Bilgi", "Mesajlar sayfası henüz yapım aşamasında.", "Tamam");
    }

    private async void OnSearchTapped(object sender, EventArgs e)
    {
        await AnimateIcon(sender as View);
        await Application.Current.MainPage.DisplayAlert("Bilgi", "Eğitmen keşfet sayfası açılacak.", "Tamam");
    }

    private async void OnFavoritesTapped(object sender, EventArgs e)
    {
        await AnimateIcon(sender as View);
        await Application.Current.MainPage.DisplayAlert("Bilgi", "Derslerim / favoriler sayfası henüz yapım aşamasında.", "Tamam");
    }

    private async void OnProfileTapped(object sender, EventArgs e)
    {
        await AnimateIcon(sender as View);

        var services = Handler?.MauiContext?.Services;
        var profilePage = await ProfileNavigationHelper.CreateCurrentUserProfilePageAsync(services);

        if (profilePage is null)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Bilgi",
                "Profil sayfası için oturum bilgisi alınamadı. Lütfen tekrar giriş yapın.",
                "Tamam");
            return;
        }

        if (Application.Current.MainPage is FlyoutPage flyout)
        {
            flyout.Detail = new NavigationPage(profilePage);
            return;
        }

        await Navigation.PushAsync(profilePage);
    }

    private async Task AnimateIcon(View? view)
    {
        if (view == null)
            return;

        await view.ScaleTo(0.8, 100, Easing.CubicOut);
        await view.ScaleTo(1.0, 100, Easing.CubicIn);
    }
}
