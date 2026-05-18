// Views/Pages/BatchDetailsPage.xaml.cs
using System.Windows.Controls;
using TechModule.Models;
using TechModule.ViewModels;

namespace TechModule.Views.Pages
{
    public partial class BatchDetailsPage : Page
    {
        private BatchDetailsViewModel _vm;
        public BatchDetailsPage(Batch batch)
        {
            InitializeComponent();
            _vm = new BatchDetailsViewModel(batch);
            DataContext = _vm;
            Loaded += async (s, e) => await _vm.LoadDetails();
        }
    }
}