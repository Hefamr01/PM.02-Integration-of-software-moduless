using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TechModule.Models;
using TechModule.Services;

namespace TechModule.ViewModels
{
    public class BatchesViewModel : BaseViewModel
    {
        public ObservableCollection<Batch> Batches { get; } = new ObservableCollection<Batch>();
        private Batch _selectedBatch;
        public Batch SelectedBatch
        {
            get => _selectedBatch;
            set
            {
                Set(ref _selectedBatch, value);
                ((RelayCommand)StartCommand)?.CanExecute(null);
                ((RelayCommand)CompleteCommand)?.CanExecute(null);
                ((RelayCommand)ViewDetailsCommand)?.CanExecute(null);
            }
        }

        public ICommand LoadCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand CompleteCommand { get; }
        public ICommand ViewDetailsCommand { get; }

        public Action<Batch> OnViewDetails { get; set; }
        public Func<Task> OnRefresh { get; set; }

        public BatchesViewModel()
        {
            LoadCommand = new RelayCommand(async _ => await LoadBatches());
            AddCommand = new RelayCommand(async _ => await CreateBatch());
            StartCommand = new RelayCommand(async _ => await StartBatch(), _ => SelectedBatch != null && SelectedBatch.status == "planned");
            CompleteCommand = new RelayCommand(async _ => await CompleteBatch(), _ => SelectedBatch != null && SelectedBatch.status == "running");
            ViewDetailsCommand = new RelayCommand(_ => OnViewDetails?.Invoke(SelectedBatch), _ => SelectedBatch != null);
        }

        public async Task LoadBatches()
        {
            try
            {
                var list = await ApiService.GetAsync<System.Collections.Generic.List<Batch>>("batches");
                Batches.Clear();
                foreach (var b in list) Batches.Add(b);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки партий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CreateBatch()
        {
            try
            {
                var newBatch = new Batch
                {
                    status = "planned",
                    batch_number = $"B{DateTime.Now:yyyyMMddHHmmss}",
                    // Обязательные поля – предполагается, что в БД есть записи с id = 1
                    order_id = 1,
                    recipe_id = 1,
                    tech_map_id = 1
                };
                var created = await ApiService.CreateBatchAsync(newBatch);
                Batches.Add(created);
                await LoadBatches();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания партии: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task StartBatch()
        {
            try
            {
                var userId = App.CurrentUser?.id ?? 1;
                await ApiService.StartBatchAsync<Batch>(SelectedBatch.id, userId, null);
                await LoadBatches();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка старта партии: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CompleteBatch()
        {
            try
            {
                var userId = App.CurrentUser?.id ?? 1;
                await ApiService.CompleteBatchAsync<Batch>(SelectedBatch.id, userId, null);
                await LoadBatches();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка завершения партии: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}