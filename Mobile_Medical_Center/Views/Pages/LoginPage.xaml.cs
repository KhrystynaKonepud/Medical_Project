using Mobile_Medical_Center.ViewModels;

namespace Mobile_Medical_Center.Views.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        BindingContext = new LoginViewModel();
    }
}
