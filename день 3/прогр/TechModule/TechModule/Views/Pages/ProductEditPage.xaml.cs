using System.Windows;
using System.Windows.Controls;
using TechModule.Models;
using TechModule.ViewModels;

namespace TechModule.Views.Pages
{
    public partial class ProductEditPage : Page
    {
        private ProductEditViewModel _vm;
        public ProductEditPage(Product product = null)
        {
            InitializeComponent();
            _vm = new ProductEditViewModel(product);
            _vm.OnSaved = () =>
            {
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
            };
            DataContext = _vm;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}