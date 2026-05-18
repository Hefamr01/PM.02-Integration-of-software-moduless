using System.Windows.Controls;
using TechModule.ViewModels;

namespace TechModule.Views.Pages
{
    public partial class DashboardPage : Page
    {
        private DashboardViewModel _vm;
        public DashboardPage()
        {
            InitializeComponent();
            _vm = new DashboardViewModel();
            DataContext = _vm;
            Loaded += async (s, e) => await _vm.LoadDashboard();
        }
    }
}