// Views/Pages/TechMapsPage.xaml.cs
using System.Windows.Controls;
using TechModule.ViewModels;

namespace TechModule.Views.Pages
{
    public partial class TechMapsPage : Page
    {
        private TechMapsViewModel _vm;
        public TechMapsPage()
        {
            InitializeComponent();
            _vm = new TechMapsViewModel();
            _vm.OnEditTechMap = map =>
            {
                var editPage = new TechMapEditPage(map);
                NavigationService.Navigate(editPage);
            };
            DataContext = _vm;
            Loaded += async (s, e) => await _vm.LoadTechMaps();
        }
    }
}