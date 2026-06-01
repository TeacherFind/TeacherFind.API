using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using TeacherFind.Mobile.Features.Profile.Models;
using TeacherFind.Mobile.Features.Profile.Views;
using TeacherFind.Mobile.Features.Home.Views;
namespace TeacherFind.Mobile.Shared.Components;

public partial class MainFlyoutMenuPage : ContentPage
{
    public event Action<ProfileMenuItemModel> OnMenuItemSelected;

    public MainFlyoutMenuPage()
    {
        InitializeComponent();

        // Senin klasöründeki gerçek sayfa tiplerini buraya eþliyoruz
        var menuItems = new List<ProfileMenuItemModel>
        {
            new ProfileMenuItemModel { Id = "Panel", Title = "Panel", Icon = "??", TargetPage = typeof(HomePage) },
            new ProfileMenuItemModel { Id = "IlanVer", Title = "Ýlan Ver", Icon = "?", TargetPage = typeof(HomePage) },
            new ProfileMenuItemModel { Id = "Musaitlik", Title = "Müsaitlik Ayarlarý", Icon = "??", TargetPage = typeof(ProfileSettingsPage) },
            // Baþýna 'Profile' takýsýný ekleyerek listeyle tamamen eþitliyoruz:
            new ProfileMenuItemModel { Id = "Derslerim", Title = "Derslerim", Icon = "??", TargetPage = typeof(HomePage) },
            new ProfileMenuItemModel { Id = "Profilim", Title = "Profilim", Icon = "??", TargetPage = typeof(ProfilePage) },
            new ProfileMenuItemModel { Id = "Mesajlar", Title = "Mesajlar", Icon = "??", TargetPage = typeof(HomePage) },
            new ProfileMenuItemModel { Id = "Guvenlik", Title = "Güvenlik", Icon = "???", TargetPage = typeof(ProfileSettingsPage) },
            new ProfileMenuItemModel { Id = "Cikis", Title = "Çýkýþ Yap", Icon = "??", TargetPage = typeof(HomePage) }
        };

        MenuCollectionView.ItemsSource = menuItems;
        MenuCollectionView.SelectionChanged += MenuCollectionView_SelectionChanged;
    }

    private void MenuCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is ProfileMenuItemModel selectedItem)
        {
            OnMenuItemSelected?.Invoke(selectedItem);
            MenuCollectionView.SelectedItem = null;
        }
    }
}