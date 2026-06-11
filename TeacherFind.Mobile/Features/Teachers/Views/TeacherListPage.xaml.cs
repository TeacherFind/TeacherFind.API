using System;
using Microsoft.Maui.Controls;
using TeacherFind.Mobile.Features.Teachers.ViewModels;

namespace TeacherFind.Mobile.Features.Teachers.Views;

public partial class TeacherListPage : ContentPage
{
    private readonly TeacherListViewModel _viewModel;

    public TeacherListPage(TeacherListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Sayfa ekrana her geldiginde veritabanindan listeyi cekecek
        await _viewModel.LoadTeachersAsync();
    }

    private async void OnFilterButtonClicked(object sender, EventArgs e)
    {
        // Mobilde filtre ekranlari genellikle Navigation.PushModalAsync ile
        // alttan yukari dogru sik bir sekilde kayarak acilir.

        // Dogrudan 'Search' klasorundeki yeni filtremizi cagiriyoruz
        var filterPage = Handler.MauiContext.Services.GetService<TeacherFind.Mobile.Features.Search.Views.SearchFilterPage>();
        await Navigation.PushModalAsync(filterPage);
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
