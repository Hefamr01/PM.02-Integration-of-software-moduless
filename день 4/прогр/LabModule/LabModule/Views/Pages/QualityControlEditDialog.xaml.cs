using System.Windows;
using LabModule.ViewModels;

namespace LabModule.Views.Pages
{
    public partial class QualityControlEditDialog : Window
    {
        private QualityControlEditViewModel _vm;
        public QualityControlEditDialog(int ownerId, bool forRawMaterial)
        {
            InitializeComponent();
            _vm = new QualityControlEditViewModel(null, forRawMaterial);
            if (forRawMaterial)
                _vm.Control.raw_material_batch_id = ownerId;
            else
                _vm.Control.batch_id = ownerId;
            _vm.OnSaved = () => DialogResult = true;
            DataContext = _vm;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}