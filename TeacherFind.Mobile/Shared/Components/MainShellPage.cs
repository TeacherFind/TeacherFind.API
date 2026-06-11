using TeacherFind.Mobile.Core.Navigation;
using TeacherFind.Mobile.Features.Profile.Models;

namespace TeacherFind.Mobile.Shared.Components;

public class MainShellPage : FlyoutPage
{
    private readonly MainFlyoutMenuPage _menuPage;
    private readonly IServiceProvider _services;

    public MainShellPage(MainFlyoutMenuPage menuPage, IServiceProvider services)
    {
        _menuPage = menuPage;
        _services = services;

        Flyout = _menuPage;

        var initialPage = (ContentPage)_services.GetService(typeof(Features.Home.Views.HomePage));
        Detail = new NavigationPage(initialPage);

        _menuPage.OnMenuItemSelected += MenuPage_OnMenuItemSelected;
    }

    private async void MenuPage_OnMenuItemSelected(ProfileMenuItemModel item)
    {
        if (item.Id == "Cikis")
            return;

        if (item.Id == "Profilim")
        {
            var profilePage = await ProfileNavigationHelper.CreateCurrentUserProfilePageAsync(_services);
            if (profilePage is null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Bilgi",
                    "Profil sayfası için oturum bilgisi alınamadı. Lütfen tekrar giriş yapın.",
                    "Tamam");
                IsPresented = false;
                return;
            }

            Detail = new NavigationPage(profilePage);
            IsPresented = false;
            return;
        }

        var targetPageInstance = (ContentPage)_services.GetService(item.TargetPage);
        Detail = new NavigationPage(targetPageInstance);

        IsPresented = false;
    }
}
