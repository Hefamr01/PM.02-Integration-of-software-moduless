using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using OperatorModule.Helpers;
using OperatorModule.Models;
using OperatorModule.Services;

namespace OperatorModule.ViewModels
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
        public string Phone
        {
            get => _phone;
            set
            {
                if (Set(ref _phone, value))
                {
                    // Автоматическая форматирование при вводе (простая маска)
                    if (!string.IsNullOrEmpty(value))
                    {
                        string digits = Regex.Replace(value, @"\D", "");
                        if (digits.Length >= 1 && digits[0] == '7') digits = "+" + digits;
                        else if (digits.Length >= 1 && digits[0] == '8') digits = "+7" + digits.Substring(1);
                        if (digits.Length >= 2 && digits[0] == '+' && digits[1] == '7')
                        {
                            // Формат +7 (XXX) XXX-XX-XX
                            var match = Regex.Match(digits, @"^\+7(\d{0,3})(\d{0,3})(\d{0,2})(\d{0,2})$");
                            if (match.Success)
                            {
                                string formatted = "+7";
                                if (match.Groups[1].Length > 0) formatted += " (" + match.Groups[1].Value;
                                if (match.Groups[2].Length > 0) formatted += ") " + match.Groups[2].Value;
                                if (match.Groups[3].Length > 0) formatted += "-" + match.Groups[3].Value;
                                if (match.Groups[4].Length > 0) formatted += "-" + match.Groups[4].Value;
                                // Обновляем без зацикливания
                                if (formatted != value)
                                    _phone = formatted;
                            }
                        }
                    }
                    OnPropertyChanged(nameof(Phone));
                }
            }
        }

        // Роли
        public ObservableCollection<Role> Roles { get; set; } = new ObservableCollection<Role>();
        private Role _selectedRole;
        public Role SelectedRole
        {
            get => _selectedRole;
            set => Set(ref _selectedRole, value);
        }

        // Отделы
        public ObservableCollection<Department> Departments { get; set; } = new ObservableCollection<Department>();
        private Department _selectedDepartment;
        public Department SelectedDepartment
        {
            get => _selectedDepartment;
            set => Set(ref _selectedDepartment, value);
        }

        private string _error;
        public string Error { get => _error; set => Set(ref _error, value); }

        private string _message;
        public string Message { get => _message; set => Set(ref _message, value); }

        public ICommand RegisterCommand { get; }
        public ICommand LoadRolesAndDepartmentsCommand { get; }
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
            LoadRolesAndDepartmentsCommand = new RelayCommand(async _ => await LoadRolesAndDepartments());
            RefreshCaptchaCommand = new RelayCommand(_ => GenerateNewCaptcha());
            GenerateNewCaptcha();
            LoadRolesAndDepartmentsCommand.Execute(null);
        }

        private async Task LoadRolesAndDepartments()
        {
            try
            {
                var roles = await ApiService.GetRoles();
                Roles.Clear();
                foreach (var r in roles) Roles.Add(r);
                // Выбираем роль "аппаратчик" по умолчанию (id=4), если есть
                SelectedRole = Roles.FirstOrDefault(r => r.id == 4) ?? Roles.FirstOrDefault();

                var depts = await ApiService.GetDepartments();
                Departments.Clear();
                foreach (var d in depts) Departments.Add(d);
                // Отдел не выбран по умолчанию
                SelectedDepartment = null;
            }
            catch (Exception ex)
            {
                Error = $"Ошибка загрузки справочников: {ex.Message}";
            }
        }

        private async Task Register()
        {
            Error = string.Empty;
            Message = string.Empty;

            // Проверка капчи
            if (!string.Equals(UserCaptchaInput, CaptchaText, StringComparison.OrdinalIgnoreCase))
            {
                Error = "Неверный код с картинки";
                GenerateNewCaptcha();
                return;
            }

            // Валидация полей
            if (string.IsNullOrWhiteSpace(Username) || Username.Length < 3)
            {
                Error = "Логин должен содержать не менее 3 символов";
                return;
            }
            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 4)
            {
                Error = "Пароль должен содержать не менее 4 символов";
                return;
            }
            if (string.IsNullOrWhiteSpace(FullName))
            {
                Error = "Полное имя обязательно";
                return;
            }
            if (SelectedRole == null)
            {
                Error = "Выберите роль";
                return;
            }
            if (!string.IsNullOrWhiteSpace(Email) && !IsValidEmail(Email))
            {
                Error = "Некорректный email";
                return;
            }
            // Валидация телефона (необязательно, но если указан, то проверяем формат)
            if (!string.IsNullOrWhiteSpace(Phone))
            {
                string cleanPhone = Regex.Replace(Phone, @"\D", "");
                if (!Regex.IsMatch(cleanPhone, @"^(7|8)\d{10}$")) // 7 или 8 и 10 цифр после
                {
                    Error = "Телефон должен быть в формате +7XXXXXXXXXX или 8XXXXXXXXXX (11 цифр)";
                    return;
                }
            }

            try
            {
                var request = new RegisterRequest
                {
                    username = Username,
                    password = Password,
                    full_name = FullName,
                    role_id = SelectedRole.id,
                    email = Email,
                    phone = Phone,
                    department_id = SelectedDepartment?.id
                };
                var result = await ApiService.Register(request);
                if (result.success == true)
                {
                    Message = "Регистрация успешна! Выполняется автоматический вход...";
                    var loginResult = await ApiService.Login(new LoginRequest { username = Username, password = Password });
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
                        Message = "Регистрация успешна. Пожалуйста, войдите вручную.";
                        OnRegistrationSuccess?.Invoke();
                    }
                }
                else
                {
                    Error = result.message ?? "Ошибка регистрации";
                }
            }
            catch (Exception ex)
            {
                Error = $"Ошибка: {ex.Message}";
            }
        }

        private bool IsValidEmail(string email) =>
            Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);

        private void GenerateNewCaptcha()
        {
            (CaptchaText, CaptchaImage) = CaptchaGenerator.Generate();
            OnPropertyChanged(nameof(CaptchaText));
            OnPropertyChanged(nameof(CaptchaImage));
            UserCaptchaInput = string.Empty;
        }
    }
}