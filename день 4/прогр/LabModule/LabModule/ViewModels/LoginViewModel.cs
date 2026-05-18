using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using LabModule.Helpers;
using LabModule.Models;
using LabModule.Services;

namespace LabModule.ViewModels
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

        // Капча
        public string CaptchaText { get; private set; }
        private string _userCaptchaInput;
        public string UserCaptchaInput { get => _userCaptchaInput; set => Set(ref _userCaptchaInput, value); }
        public BitmapImage CaptchaImage { get; private set; }
        public ICommand RefreshCaptchaCommand { get; }

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
                Error = "Неверный код с картинки";
                GenerateNewCaptcha();
                return;
            }

            // Проверка заполнения полей
            if (string.IsNullOrWhiteSpace(Username))
            {
                Error = "Введите логин";
                return;
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                Error = "Введите пароль";
                return;
            }

            Error = string.Empty;
            try
            {
                var request = new LoginRequest { username = Username, password = Password };
                var result = await ApiService.PostAsync<AuthResponse>("auth/login", request);
                if (result.success)
                {
                    // Проверка роли: лаборант (role_id=3) или администратор (1)
                    if (result.user.role_id != 1 && result.user.role_id != 3)
                    {
                        Error = "Доступ только для лаборатории или администратора";
                        return;
                    }
                    App.CurrentUser = result.user;
                    App.AuthToken = result.token;
                    ApiService.Token = result.token;
                    ApiService.SetAuthHeader();
                    OnLoginSuccess?.Invoke();
                }
                else
                {
                    Error = result.message ?? "Неверный логин или пароль";
                }
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    Error = "Неверный логин или пароль";
                else Error = $"Ошибка: {ex.Message}";
                GenerateNewCaptcha();
            }
            catch (Exception ex)
            {
                Error = $"Ошибка соединения: {ex.Message}";
                GenerateNewCaptcha();
            }
        }

        private void GenerateNewCaptcha()
        {
            (CaptchaText, CaptchaImage) = CaptchaGenerator.Generate();
            OnPropertyChanged(nameof(CaptchaText));
            OnPropertyChanged(nameof(CaptchaImage));
            UserCaptchaInput = string.Empty;
        }
    }
}