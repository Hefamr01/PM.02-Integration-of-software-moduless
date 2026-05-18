using System.Windows;
using System.Windows.Controls;
using LabModule.ViewModels;

namespace LabModule.Views.Pages
{
    public partial class BatchesQualityPage : Page
    {
        private BatchesQualityViewModel _vm;
        public BatchesQualityPage()
        {
            InitializeComponent();
            _vm = new BatchesQualityViewModel();
            _vm.OnAddQualityControl = batch =>
            {
                var editDlg = new QualityControlEditDialog(batch.id, false);
                editDlg.Owner = Window.GetWindow(this);
                if (editDlg.ShowDialog() == true)
                    _vm.LoadBatches();
            };
            _vm.OnViewControls = batch =>
            {
                var listWin = new QualityControlsListWindow(batch.id, false);
                listWin.Owner = Window.GetWindow(this);
                listWin.ShowDialog();
            };
            DataContext = _vm;
            Loaded += async (s, e) => await _vm.LoadBatches();
        }

        private void GenerateProtocol_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedBatch == null) return;
            var protoWin = new ProtocolWindow(null, _vm.SelectedBatch.id);
            protoWin.Owner = Window.GetWindow(this);
            protoWin.ShowDialog();
        }
    }
}