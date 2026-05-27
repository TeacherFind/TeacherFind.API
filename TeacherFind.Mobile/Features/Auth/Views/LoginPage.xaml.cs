using TeacherFind.Mobile.Features.Auth.ViewModels;

namespace TeacherFind.Mobile.Features.Auth.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}