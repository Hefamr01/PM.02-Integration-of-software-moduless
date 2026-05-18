using System.Windows;
using LabModule.ViewModels;

namespace LabModule.Views.Pages
{
    public partial class ProtocolWindow : Window
    {
        public ProtocolWindow(int? rawMaterialBatchId, int? batchId)
        {
            InitializeComponent();
            DataContext = new ProtocolViewModel(rawMaterialBatchId, batchId);
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}