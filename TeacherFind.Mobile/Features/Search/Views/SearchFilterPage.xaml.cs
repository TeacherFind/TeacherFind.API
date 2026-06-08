using System;
using Microsoft.Maui.Controls;
using TeacherFind.Mobile.Features.Search.ViewModels;

namespace TeacherFind.Mobile.Features.Search.Views;

public partial class SearchFilterPage : ContentPage
{
    public SearchFilterPage(SearchFilterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnCloseButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // ViewModel'e eriþip verileri baþlat
        if (BindingContext is SearchFilterViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}