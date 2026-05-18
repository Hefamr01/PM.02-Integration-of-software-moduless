using System.Windows;
using OperatorModule.ViewModels;

namespace OperatorModule.Views
{
    public partial class LoginWindow : Window
    {
        private LoginViewModel _vm;
        public LoginWindow()
        {
            InitializeComponent();
            _vm = new LoginViewModel();
            _vm.OnLoginSuccess = () =>
            {
                var main = new MainWindow();
                main.Show();
                Close();
            };
            DataContext = _vm;
            PasswordBox.PasswordChanged += (s, e) => _vm.Password = PasswordBox.Password;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var regWin = new RegisterWindow();
            regWin.ShowDialog();
        }
    }
}