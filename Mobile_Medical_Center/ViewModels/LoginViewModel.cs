using Mobile_Medical_Center.Services;
using System.Windows.Input;

namespace Mobile_Medical_Center.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _email = "";
        private string _password = "";
        private string _errorMessage = "";
        private bool _showPassword;
        private readonly IAuthService _authService;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool ShowPassword
        {
            get => _showPassword;
            set => SetProperty(ref _showPassword, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand TogglePasswordCommand { get; }

        public LoginViewModel()
        {
            _authService = ServiceHelper.GetService<IAuthService>();

            LoginCommand = new AsyncCommand(OnLogin, CanExecuteLogin);
            RegisterCommand = new AsyncCommand(OnRegister);
            TogglePasswordCommand = new AsyncCommand(OnTogglePassword);
        }

        private async Task OnLogin()
        {
            ErrorMessage = "";
            IsLoading = true;

            try
            {
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Будь ласка, заповніть всі поля";
                    return;
                }

                var success = await _authService.LoginAsync(Email, Password);

                if (success)
                {
                    // Navigate to main app
                    await Shell.Current.GoToAsync("//dashboard");
                }
                else
                {
                    ErrorMessage = "Невірні дані для входу";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Помилка входу: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnRegister()
        {
            await Shell.Current.GoToAsync("register");
        }

        private async Task OnTogglePassword()
        {
            ShowPassword = !ShowPassword;
            await Task.CompletedTask;
        }

        private bool CanExecuteLogin()
        {
            return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
        }
    }
}
