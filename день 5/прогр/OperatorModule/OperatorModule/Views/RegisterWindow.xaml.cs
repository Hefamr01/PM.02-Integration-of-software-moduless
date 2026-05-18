using System.Windows;
using OperatorModule.ViewModels;

namespace OperatorModule.Views
{
    public partial class RegisterWindow : Window
    {
        private RegisterViewModel _vm;
        public RegisterWindow()
        {
            InitializeComponent();
            _vm = new RegisterViewModel();
            _vm.OnRegistrationSuccess = () => Close();
            DataContext = _vm;
            PasswordBox.PasswordChanged += (s, e) => _vm.Password = PasswordBox.Password;
        }
    }
}