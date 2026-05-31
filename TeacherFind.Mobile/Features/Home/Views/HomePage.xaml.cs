using Microsoft.Maui.Controls;
using TeacherFind.Mobile.Features.Home.ViewModels;

namespace TeacherFind.Mobile.Features.Home.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Sayfa her açýldýðýnda veritabanýndan verileri asenkron olarak sorgular
        await _viewModel.InitializeAsync();
    }
    // ÝÞTE MAVÝ ALANDAKÝ BUTONA BASILDIÐINDA SOL MENÜYÜ AÇAN MOTOR
    private void OnMenuButtonClicked(object sender, EventArgs e)
    {
        // 'App' kelimesi projedeki klasörle karýþtýðý için doðrudan 'Application' kullanýyoruz.
        if (Application.Current.MainPage is FlyoutPage flyoutPage)
        {
            // Menü kapalýysa açar, açýksa kapatýr
            flyoutPage.IsPresented = !flyoutPage.IsPresented;
        }
    }
    private async void OnSearchTapped(object sender, EventArgs e)
    {
        var teacherListPage = Handler.MauiContext.Services.GetService<TeacherFind.Mobile.Features.Teachers.Views.TeacherListPage>();
        await Navigation.PushAsync(teacherListPage);
    }
}