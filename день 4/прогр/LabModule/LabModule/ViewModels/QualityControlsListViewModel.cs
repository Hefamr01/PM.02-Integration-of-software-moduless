using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LabModule.Models;
using LabModule.Services;

namespace LabModule.ViewModels
{
    public class QualityControlsListViewModel : BaseViewModel
    {
        public ObservableCollection<QualityControl> Controls { get; } = new ObservableCollection<QualityControl>();
        private QualityControl _selectedControl;
        public QualityControl SelectedControl
        {
            get => _selectedControl;
            set
            {
                Set(ref _selectedControl, value);
                ((RelayCommand)TakeDecisionCommand)?.CanExecute(null);
            }
        }

        private string _comment;
        public string Comment { get => _comment; set => Set(ref _comment, value); }

        public ICommand LoadCommand { get; }
        public ICommand TakeDecisionCommand { get; }

        private int _ownerId; // batch_id или raw_material_batch_id
        private bool _isForRawMaterial;

        public QualityControlsListViewModel(int ownerId, bool forRawMaterial)
        {
            _ownerId = ownerId;
            _isForRawMaterial = forRawMaterial;
            LoadCommand = new RelayCommand(async _ => await LoadControls());
            TakeDecisionCommand = new RelayCommand(async param => await TakeDecision(param?.ToString()),
                _ => SelectedControl != null && string.IsNullOrEmpty(SelectedControl.decision));
        }

        private async Task LoadControls()
        {
            try
            {
                var all = await ApiService.GetQualityControls();
                var list = _isForRawMaterial ?
                    all.Where(q => q.raw_material_batch_id == _ownerId).ToList() :
                    all.Where(q => q.batch_id == _ownerId).ToList();
                Controls.Clear();
                foreach (var c in list) Controls.Add(c);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task TakeDecision(string decision)
        {
            if (decision != "approved" && decision != "blocked") return;
            if (decision == "blocked" && string.IsNullOrWhiteSpace(Comment))
            {
                MessageBox.Show("При блокировке необходимо указать комментарий.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var updated = await ApiService.SetQualityControlDecision(SelectedControl.id, App.CurrentUser.id, decision, Comment);
                // Обновить локально
                SelectedControl.decision = updated.decision;
                SelectedControl.result = updated.result;
                SelectedControl.analyst_comment = updated.analyst_comment;
                OnPropertyChanged(nameof(Controls));
                Comment = "";
                await LoadControls(); // перезагрузка для обновления
                MessageBox.Show($"Решение '{decision}' принято.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}