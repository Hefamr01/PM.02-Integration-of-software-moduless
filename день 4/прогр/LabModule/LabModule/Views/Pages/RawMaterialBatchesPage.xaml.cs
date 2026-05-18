using System.Windows;
using System.Windows.Controls;
using LabModule.Models;
using LabModule.ViewModels;

namespace LabModule.Views.Pages
{
    public partial class RawMaterialBatchesPage : Page
    {
        private RawMaterialBatchesViewModel _vm;
        public RawMaterialBatchesPage()
        {
            InitializeComponent();
            _vm = new RawMaterialBatchesViewModel();
            _vm.OnAddQualityControl = batch =>
            {
                var editDlg = new QualityControlEditDialog(batch.id, true);
                editDlg.Owner = Window.GetWindow(this);
                if (editDlg.ShowDialog() == true)
                    _vm.LoadBatches();
            };
            _vm.OnViewControls = batch =>
            {
                var listWin = new QualityControlsListWindow(batch.id, true);
                listWin.Owner = Window.GetWindow(this);
                listWin.ShowDialog();
            };
            DataContext = _vm;
            Loaded += async (s, e) => await _vm.LoadBatches();
        }

        private void GenerateProtocol_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedBatch == null) return;
            var protoWin = new ProtocolWindow(_vm.SelectedBatch.id, null);
            protoWin.Owner = Window.GetWindow(this);
            protoWin.ShowDialog();
        }
    }
}