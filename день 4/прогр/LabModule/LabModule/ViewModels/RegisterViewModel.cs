using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using LabModule.Helpers;
using LabModule.Models;
using LabModule.Services;

namespace LabModule.ViewModels
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

        private int _roleId = 3; // по умолчанию лаборант
        public int RoleId { get => _roleId; set => Set(ref _roleId, value); }

        private int? _departmentId;
        public int? DepartmentId { get => _departmentId; set => Set(ref _departmentId, value); }

        private string _error;
        public string Error { get => _error; set => Set(ref _error, value); }

        private string _message;
        public string Message { get => _message; set => Set(ref _message, value); }

        public ICommand RegisterCommand { get; }
        public Action OnRegistrationSuccess { get; set; }

        // Капча
        public string CaptchaText { get; private set; }
        private string _userCaptchaInput;
        public string UserCaptchaInput { get => _userCaptchaInput; set => Set(ref _userCaptchaInput, value); }
        public BitmapImage CaptchaImage { get; private set; }
        public ICommand RefreshCaptchaCommand { get; }

        public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand(async _ => await Register());
            RefreshCaptchaCommand = new RelayCommand(_ => GenerateNewCaptcha());
            GenerateNewCaptcha();
        }

        private async Task Register()
        {
            // Сброс сообщений
            Error = string.Empty;
            Message = string.Empty;

            // 1. Проверка капчи
            if (!string.Equals(UserCaptchaInput, CaptchaText, StringComparison.OrdinalIgnoreCase))
            {
                Error = "Неверный код с картинки";
                GenerateNewCaptcha();
                return;
            }

            // 2. Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(Username))
            {
                Error = "Логин обязателен для заполнения";
                return;
            }
            if (Username.Length < 3)
            {
                Error = "Логин должен содержать не менее 3 символов";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                Error = "Пароль обязателен для заполнения";
                return;
            }
            if (Password.Length < 4)
            {
                Error = "Пароль должен содержать не менее 4 символов";
                return;
            }

            if (string.IsNullOrWhiteSpace(FullName))
            {
                Error = "Полное имя обязательно для заполнения";
                return;
            }

            // 3. Проверка роли (должна быть 1, 2 или 3, но лаборанту лучше дать 3)
            if (RoleId != 1 && RoleId != 2 && RoleId != 3)
            {
                Error = "Роль должна быть 1 (админ), 2 (технолог) или 3 (лаборант)";
                return;
            }

            // 4. Проверка email (если заполнен)
            if (!string.IsNullOrWhiteSpace(Email))
            {
                if (!IsValidEmail(Email))
                {
                    Error = "Некорректный формат email";
                    return;
                }
            }

            // 5. Проверка телефона (необязательно, можно пропустить)
            // 6. Проверка department_id (если указан, должен быть положительным числом)
            if (DepartmentId.HasValue && DepartmentId.Value <= 0)
            {
                Error = "ID отдела должен быть положительным числом";
                return;
            }

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
                // Предполагается, что при успехе сервер возвращает { success = true, id = ... }
                if (result.success == true)
                {
                    Message = "Регистрация прошла успешно! Выполняется автоматический вход...";

                    // Автоматический вход
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
                        // Если автоматический вход не удался, всё равно считаем регистрацию успешной
                        Message = "Регистрация успешна. Пожалуйста, войдите вручную.";
                        OnRegistrationSuccess?.Invoke(); // закрываем окно регистрации, чтобы пользователь залогинился отдельно
                    }
                }
                else
                {
                    Error = result.message ?? "Ошибка регистрации. Возможно, пользователь уже существует.";
                }
            }
            catch (Exception ex)
            {
                Error = $"Ошибка: {ex.Message}";
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
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