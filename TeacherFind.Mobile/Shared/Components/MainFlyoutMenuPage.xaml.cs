using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using TeacherFind.Mobile.Features.Profile.Models;
using TeacherFind.Mobile.Features.Profile.Views;
using TeacherFind.Mobile.Features.Home.Views;
using TeacherFind.Mobile.Features.Auth.Views;

namespace TeacherFind.Mobile.Shared.Components;

public partial class MainFlyoutMenuPage : ContentPage
{
    public event Action<ProfileMenuItemModel> OnMenuItemSelected;

    public MainFlyoutMenuPage()
    {
        InitializeComponent();

        var menuItems = new List<ProfileMenuItemModel>
        {
            // Login eklendi
            new ProfileMenuItemModel { Id = "Login", Title = "Giriş Yap", Icon = "🔑", TargetPage = typeof(LoginPage) },
            
            new ProfileMenuItemModel { Id = "Panel", Title = "Panel", Icon = "📊", TargetPage = typeof(HomePage) },
            new ProfileMenuItemModel { Id = "IlanVer", Title = "İlan Ver", Icon = "📢", TargetPage = typeof(HomePage) },
            new ProfileMenuItemModel { Id = "Musaitlik", Title = "Müsaitlik Ayarları", Icon = "⏱", TargetPage = typeof(ProfileSettingsPage) },
            new ProfileMenuItemModel { Id = "Derslerim", Title = "Derslerim", Icon = "📚", TargetPage = typeof(HomePage) },
            new ProfileMenuItemModel { Id = "Profilim", Title = "Profilim", Icon = "👤", TargetPage = typeof(ProfilePage) },
            new ProfileMenuItemModel { Id = "Mesajlar", Title = "Mesajlar", Icon = "💬", TargetPage = typeof(HomePage) },
            new ProfileMenuItemModel { Id = "Guvenlik", Title = "Güvenlik", Icon = "🔒", TargetPage = typeof(ProfileSettingsPage) },
            new ProfileMenuItemModel { Id = "Cikis", Title = "Çıkış Yap", Icon = "🚪", TargetPage = typeof(HomePage) }
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
