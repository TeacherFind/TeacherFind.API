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

        // Sayfa ekrana her geldiðinde veritabanýndan listeyi çekecek
        await _viewModel.LoadTeachersAsync();
    }
}