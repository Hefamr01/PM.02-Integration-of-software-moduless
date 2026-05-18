// Views/Pages/BatchesPage.xaml.cs
using System.Windows.Controls;
using TechModule.ViewModels;

namespace TechModule.Views.Pages
{
    public partial class BatchesPage : Page
    {
        private BatchesViewModel _vm;
        public BatchesPage()
        {
            InitializeComponent();
            _vm = new BatchesViewModel();
            _vm.OnViewDetails = batch =>
            {
                var detailsPage = new BatchDetailsPage(batch);
                NavigationService.Navigate(detailsPage);
            };
            DataContext = _vm;
            Loaded += async (s, e) => await _vm.LoadBatches();
        }
    }
}