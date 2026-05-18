using System.Windows.Controls;
using TechModule.ViewModels;

namespace TechModule.Views.Pages
{
    public partial class ProductsPage : Page
    {
        private ProductsViewModel _vm;
        public ProductsPage()
        {
            InitializeComponent();
            _vm = new ProductsViewModel();
            _vm.OnEditProduct = product =>
            {
                var editPage = new ProductEditPage(product);
                NavigationService.Navigate(editPage);
            };
            DataContext = _vm;
            Loaded += async (s, e) => await _vm.LoadProducts();
        }
    }
}