using System.Windows;
using LabModule.ViewModels;
using LabModule.Views.Pages;

namespace LabModule.Views
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
                    case "RawMaterialBatches":
                        MainFrame.Navigate(new RawMaterialBatchesPage());
                        break;
                    case "BatchesQuality":
                        MainFrame.Navigate(new BatchesQualityPage());
                        break;
                }
            };
            ViewModel.NavigateCommand.Execute("RawMaterialBatches");
        }
    }
}