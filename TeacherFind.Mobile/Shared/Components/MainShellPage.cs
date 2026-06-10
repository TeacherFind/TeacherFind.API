using System;
using Microsoft.Maui.Controls;
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

        // Uygulama ilk açıldığında sağ tarafta HomePage (Panel) dursun
        var initialPage = (ContentPage)_services.GetService(typeof(Features.Home.Views.HomePage));
        Detail = new NavigationPage(initialPage);

        _menuPage.OnMenuItemSelected += MenuPage_OnMenuItemSelected;
    }

    private void MenuPage_OnMenuItemSelected(ProfileMenuItemModel item)
    {
        if (item.Id == "Cikis")
        {
            return;
        }

        Type targetType = item.TargetPage;
        if (item.Id == "Profilim")
        {
            targetType = MainApp.IsTutor ? typeof(Features.Profile.Views.TutorProfilePage) : typeof(Features.Profile.Views.StudentProfilePage);
        }

        // Tıklanan sayfayı senin katmandan (DI) otomatik üretip ekrana basıyor
        var targetPageInstance = (ContentPage)_services.GetService(targetType);
        Detail = new NavigationPage(targetPageInstance);

        IsPresented = false; // Menüyü kapat
    }
}