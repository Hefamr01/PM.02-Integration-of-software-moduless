using System.Windows;
using System.Windows.Controls;
using TechModule.Models;
using TechModule.ViewModels;

namespace TechModule.Views.Pages
{
    public partial class TechMapEditPage : Page
    {
        private TechMapEditViewModel _vm;
        public TechMapEditPage(TechMap map = null)
        {
            InitializeComponent();
            _vm = new TechMapEditViewModel(map);
            _vm.OnSaved = () =>
            {
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
            };
            DataContext = _vm;
            Loaded += async (s, e) => await _vm.InitializeAsync();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}