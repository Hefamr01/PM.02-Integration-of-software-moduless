using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using OperatorModule.Models;
using OperatorModule.Services;

namespace OperatorModule.ViewModels
{
    public class ActiveBatchesViewModel : BaseViewModel
    {
        public ObservableCollection<Batch> Batches { get; } = new ObservableCollection<Batch>();
        private Batch _selectedBatch;
        public Batch SelectedBatch
        {
            get => _selectedBatch;
            set
            {
                Set(ref _selectedBatch, value);
                ((RelayCommand)StartBatchCommand)?.CanExecute(null);
                ((RelayCommand)ExecuteBatchCommand)?.CanExecute(null);
            }
        }

        public ICommand LoadCommand { get; }
        public ICommand StartBatchCommand { get; }
        public ICommand ExecuteBatchCommand { get; }

        public Action<Batch> OnStartBatch { get; set; }
        public Action<Batch> OnExecuteBatch { get; set; }

        public ActiveBatchesViewModel()
        {
            LoadCommand = new RelayCommand(async _ => await LoadBatches());
            StartBatchCommand = new RelayCommand(async _ => await StartBatch(), _ => SelectedBatch != null && SelectedBatch.status == "planned");
            ExecuteBatchCommand = new RelayCommand(_ => OnExecuteBatch?.Invoke(SelectedBatch), _ => SelectedBatch != null && SelectedBatch.status == "running");
        }

        public async Task LoadBatches()
        {
            try
            {
                var all = await ApiService.GetBatches();
                var active = all.Where(b => b.status == "planned" || b.status == "running").ToList();
                Batches.Clear();
                foreach (var b in active) Batches.Add(b);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки партий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task StartBatch()
        {
            try
            {
                var result = await ApiService.StartBatch(SelectedBatch.id, null);
                await LoadBatches();
                MessageBox.Show($"Партия {result.batch_number} запущена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                OnExecuteBatch?.Invoke(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}