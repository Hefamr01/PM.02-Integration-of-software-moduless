using System.Windows;
using System.Windows.Controls;
using OperatorModule.Models;
using OperatorModule.ViewModels;

namespace OperatorModule.Views.Pages
{
    public partial class ActiveBatchesPage : Page
    {
        private ActiveBatchesViewModel _vm;
        public ActiveBatchesPage()
        {
            InitializeComponent();
            _vm = new ActiveBatchesViewModel();
            _vm.OnExecuteBatch = batch =>
            {
                var execWin = new BatchExecutionPage(batch);
                var window = new Window
                {
                    Title = $"Выполнение партии {batch.batch_number}",
                    Content = execWin,
                    Width = 900,
                    Height = 700,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = Window.GetWindow(this)
                };
                execWin.OnBatchCompleted = () => { window.Close(); _vm.LoadBatches(); };
                window.ShowDialog();
            };
            _vm.OnStartBatch = batch => _vm.LoadBatches();
            DataContext = _vm;
            Loaded += async (s, e) => await _vm.LoadBatches();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) => _vm.LoadBatches();
    }
}