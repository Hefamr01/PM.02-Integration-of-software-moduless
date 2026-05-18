using System.Windows;
using System.Windows.Controls;
using TechModule.Models;
using TechModule.ViewModels;

namespace TechModule.Views.Pages
{
    public partial class ProductionOrderEditPage : Page
    {
        private ProductionOrderEditViewModel _vm;
        public ProductionOrderEditPage(ProductionOrder order = null)
        {
            InitializeComponent();
            _vm = new ProductionOrderEditViewModel(order);
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