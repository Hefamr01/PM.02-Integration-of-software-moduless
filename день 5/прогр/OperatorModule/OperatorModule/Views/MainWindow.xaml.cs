using System.Windows;
using OperatorModule.ViewModels;
using OperatorModule.Views.Pages;

namespace OperatorModule.Views
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
                if (page == "ActiveBatches")
                    MainFrame.Navigate(new ActiveBatchesPage());
            };
            ViewModel.NavigateCommand.Execute("ActiveBatches");
        }
    }
}