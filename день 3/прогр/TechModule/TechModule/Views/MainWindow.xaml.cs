using System.Windows;
using System.Windows.Controls;
using TechModule.ViewModels;
using TechModule.Views.Pages;

namespace TechModule.Views
{
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
            DataContext = ViewModel;
            ViewModel.Navigate += page =>
            {
                switch (page)
                {
                    case "Dashboard": MainFrame.Navigate(new DashboardPage()); break;
                    case "Products": MainFrame.Navigate(new ProductsPage()); break;
                    case "Recipes": MainFrame.Navigate(new RecipesPage()); break;
                    case "TechMaps": MainFrame.Navigate(new TechMapsPage()); break;
                    case "Orders": MainFrame.Navigate(new ProductionOrdersPage()); break;
                    case "Batches": MainFrame.Navigate(new BatchesPage()); break;
                }
            };
            ViewModel.NavigateCommand.Execute("Dashboard");
        }
    }
}