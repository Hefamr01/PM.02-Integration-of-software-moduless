using System.Windows;
using LabModule.ViewModels;

namespace LabModule.Views
{
    public partial class RegisterWindow : Window
    {
        private RegisterViewModel _vm;
        public RegisterWindow()
        {
            InitializeComponent();
            _vm = new RegisterViewModel();
            _vm.OnRegistrationSuccess = () => this.Close();
            DataContext = _vm;
            PasswordBox.PasswordChanged += (s, e) => _vm.Password = PasswordBox.Password;
        }
    }
}