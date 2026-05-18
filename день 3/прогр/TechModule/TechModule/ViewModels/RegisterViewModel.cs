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
    public class RegisterViewModel : BaseViewModel
    {
        private string _username;
        public string Username { get => _username; set => Set(ref _username, value); }

        private string _password;
        public string Password { get => _password; set => Set(ref _password, value); }

        private string _fullName;
        public string FullName { get => _fullName; set => Set(ref _fullName, value); }

        private string _email;
        public string Email { get => _email; set => Set(ref _email, value); }

        private string _phone;
        public string Phone { get => _phone; set => Set(ref _phone, value); }

        private int _roleId = 2;
        public int RoleId { get => _roleId; set => Set(ref _roleId, value); }

        private int? _departmentId;
        public int? DepartmentId { get => _departmentId; set => Set(ref _departmentId, value); }

        private string _error;
        public string Error { get => _error; set => Set(ref _error, value); }

        private string _message;
        public string Message { get => _message; set => Set(ref _message, value); }

        public ICommand RegisterCommand { get; }
        public Action OnRegistrationSuccess { get; set; }

        public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand(async _ => await Register());
            RefreshCaptchaCommand = new RelayCommand(_ => GenerateNewCaptcha());
            GenerateNewCaptcha();
        }

        private async Task Register()
        {
            if (!string.Equals(UserCaptchaInput, CaptchaText, StringComparison.OrdinalIgnoreCase))
            {
                Error = "Неверная капча";
                GenerateNewCaptcha();
                return;
            }

            Error = string.Empty;
            Message = string.Empty;
            try
            {
                var request = new RegisterRequest
                {
                    username = Username,
                    password = Password,
                    full_name = FullName,
                    role_id = RoleId,
                    email = Email,
                    phone = Phone,
                    department_id = DepartmentId
                };
                var result = await ApiService.PostAsync<dynamic>("auth/register", request);

                // Автоматический вход после регистрации
                var loginResult = await ApiService.PostAsync<AuthResponse>("auth/login", new LoginRequest
                {
                    username = Username,
                    password = Password
                });

                if (loginResult.success)
                {
                    App.CurrentUser = loginResult.user;
                    App.AuthToken = loginResult.token;
                    ApiService.Token = loginResult.token;
                    ApiService.SetAuthHeader();
                    OnRegistrationSuccess?.Invoke();
                }
                else
                {
                    Message = "Регистрация прошла, но не удалось войти автоматически. Пожалуйста, войдите вручную.";
                    Error = loginResult.message;
                    OnRegistrationSuccess?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Error = $"Ошибка: {ex.Message}";
                MessageBox.Show(Error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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