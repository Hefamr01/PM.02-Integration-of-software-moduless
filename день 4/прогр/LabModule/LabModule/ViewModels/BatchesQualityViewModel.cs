using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LabModule.Models;
using LabModule.Services;

namespace LabModule.ViewModels
{
    public class BatchesQualityViewModel : BaseViewModel
    {
        public ObservableCollection<Batch> Batches { get; } = new ObservableCollection<Batch>();
        private Batch _selectedBatch;
        public Batch SelectedBatch
        {
            get => _selectedBatch;
            set
            {
                Set(ref _selectedBatch, value);
                ((RelayCommand)AddQualityControlCommand)?.CanExecute(null);
                ((RelayCommand)ViewControlsCommand)?.CanExecute(null);
            }
        }

        public ICommand LoadCommand { get; }
        public ICommand AddQualityControlCommand { get; }
        public ICommand ViewControlsCommand { get; }

        public Action<Batch> OnAddQualityControl { get; set; }
        public Action<Batch> OnViewControls { get; set; }

        public BatchesQualityViewModel()
        {
            LoadCommand = new RelayCommand(async _ => await LoadBatches());
            AddQualityControlCommand = new RelayCommand(_ => OnAddQualityControl?.Invoke(SelectedBatch), _ => SelectedBatch != null);
            ViewControlsCommand = new RelayCommand(_ => OnViewControls?.Invoke(SelectedBatch), _ => SelectedBatch != null);
        }

        public async Task LoadBatches()
        {
            try
            {
                var all = await ApiService.GetBatches();
                // Показываем завершённые партии (или любые, но обычно контроль после завершения)
                var list = all.Where(b => b.status == "completed").ToList();
                Batches.Clear();
                foreach (var b in list) Batches.Add(b);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки партий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}