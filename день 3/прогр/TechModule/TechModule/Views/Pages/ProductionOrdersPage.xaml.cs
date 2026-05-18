// Views/Pages/ProductionOrdersPage.xaml.cs
using System.Windows.Controls;
using TechModule.ViewModels;

namespace TechModule.Views.Pages
{
    public partial class ProductionOrdersPage : Page
    {
        private ProductionOrdersViewModel _vm;
        public ProductionOrdersPage()
        {
            InitializeComponent();
            _vm = new ProductionOrdersViewModel();
            _vm.OnEditOrder = order =>
            {
                var editPage = new ProductionOrderEditPage(order);
                NavigationService.Navigate(editPage);
            };
            DataContext = _vm;
            Loaded += async (s, e) => await _vm.LoadOrders();
        }
    }
}