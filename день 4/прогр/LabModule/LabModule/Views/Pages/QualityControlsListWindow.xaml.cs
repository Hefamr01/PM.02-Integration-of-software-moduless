using System.Windows;
using LabModule.ViewModels;

namespace LabModule.Views.Pages
{
    public partial class QualityControlsListWindow : Window
    {
        private QualityControlsListViewModel _vm;
        public QualityControlsListWindow(int ownerId, bool forRawMaterial)
        {
            InitializeComponent();
            _vm = new QualityControlsListViewModel(ownerId, forRawMaterial);
            DataContext = _vm;
            Loaded += async (s, e) => _vm.LoadCommand.Execute(null);
            // Связь комментария
            CommentBox.TextChanged += (s, e) => _vm.Comment = CommentBox.Text;
        }
    }
}