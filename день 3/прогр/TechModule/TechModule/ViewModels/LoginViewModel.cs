using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TechModule.Helpers;
using TechModule.Models;
using TechModule.Services;

namespace TechModule.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        public string Username { get => _username; set => Set(ref _username, value); }

        private string _password;
        public string Password { get => _password; set => Set(ref _password, value); }

        private string _error;
        public string Error { get => _error; set => Set(ref _error, value); }

        public ICommand LoginCommand { get; }
        public Action OnLoginSuccess { get; set; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(async _ => await Login());
            RefreshCaptchaCommand = new RelayCommand(_ => GenerateNewCaptcha());
            GenerateNewCaptcha();
        }

        private async Task Login()
        {
            // Проверка капчи

            if (!string.Equals(UserCaptchaInput, CaptchaText, StringComparison.OrdinalIgnoreCase))
            {
                Error = "Неверная капча";
                GenerateNewCaptcha();
                return;
            }

            Error = string.Empty;
            try
            {
                var request = new LoginRequest { username = Username, password = Password };
                var result = await ApiService.PostAsync<AuthResponse>("auth/login", request);
                if (result.success)
                {
                    App.CurrentUser = result.user;
                    App.AuthToken = result.token;
                    ApiService.Token = result.token;
                    ApiService.SetAuthHeader();
                    OnLoginSuccess?.Invoke();
                }
                else
                {
                    Error = result.message ?? "Ошибка входа";
                    MessageBox.Show(Error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Error = $"Ошибка соединения: {ex.Message}";
                MessageBox.Show(Error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                GenerateNewCaptcha();
            }
        }

        public string CaptchaText { get; private set; }
        private string _userCaptchaInput;
        public string UserCaptchaInput { get => _userCaptchaInput; set => Set(ref _userCaptchaInput, value); }
        public BitmapImage CaptchaImage { get; private set; }
        public ICommand RefreshCaptchaCommand { get; }

        private void GenerateNewCaptcha()
        {
            (CaptchaText, CaptchaImage) = CaptchaGenerator.Generate();
            OnPropertyChanged(nameof(CaptchaText));
            OnPropertyChanged(nameof(CaptchaImage));
            UserCaptchaInput = string.Empty;
        }
    }
}