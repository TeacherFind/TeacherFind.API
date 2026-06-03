using System;
using Microsoft.Maui.Controls;
using TeacherFind.Mobile.Features.Home.ViewModels;

namespace TeacherFind.Mobile.Features.Home.Views;

public partial class HomePage : ContentPage
{
    // 1. Dýþarýdan Motorumuzu (HomeViewModel) Ýstiyoruz
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();

        // 2. ÝÞTE EKSÝK OLAN KRÝTÝK SATIR: Ekranýn bu motoru kullanmasýný söylüyoruz
        BindingContext = viewModel;
    }

    // 3. Sayfa ekrana geldiðinde motorun internete çýkýp verileri çekmesini tetikliyoruz
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is HomeViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }

    // Arama Butonuna Týklanma Olayý
    private async void OnSearchTapped(object sender, EventArgs e)
    {
        await Task.Delay(100);

        var teacherListPage = Handler.MauiContext.Services.GetService<TeacherFind.Mobile.Features.Teachers.Views.TeacherListPage>();

        if (teacherListPage != null)
        {
            await Navigation.PushAsync(teacherListPage);
        }
    }

    // Menü Butonuna Týklanma Olayý (Eðer varsa)
    private void OnMenuButtonClicked(object sender, EventArgs e)
    {
        if (Application.Current.MainPage is FlyoutPage flyoutPage)
        {
            flyoutPage.IsPresented = true;
        }
    }
}