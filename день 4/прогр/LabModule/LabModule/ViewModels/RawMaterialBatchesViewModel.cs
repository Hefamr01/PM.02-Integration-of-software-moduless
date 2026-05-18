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
    public class RawMaterialBatchesViewModel : BaseViewModel
    {
        public ObservableCollection<RawMaterialBatch> Batches { get; } = new ObservableCollection<RawMaterialBatch>();
        private RawMaterialBatch _selectedBatch;
        public RawMaterialBatch SelectedBatch
        {
            get => _selectedBatch;
            set
            {
                Set(ref _selectedBatch, value);
                ((RelayCommand)StartAnalysisCommand)?.CanExecute(null);
                ((RelayCommand)AddQualityControlCommand)?.CanExecute(null);
                ((RelayCommand)ApproveCommand)?.CanExecute(null);
                ((RelayCommand)BlockCommand)?.CanExecute(null);
                ((RelayCommand)ViewControlsCommand)?.CanExecute(null);
            }
        }

        public ICommand LoadCommand { get; }
        public ICommand StartAnalysisCommand { get; }
        public ICommand AddQualityControlCommand { get; }
        public ICommand ApproveCommand { get; }
        public ICommand BlockCommand { get; }
        public ICommand ViewControlsCommand { get; }

        public Action<RawMaterialBatch> OnAddQualityControl { get; set; }
        public Action<RawMaterialBatch> OnViewControls { get; set; }

        public RawMaterialBatchesViewModel()
        {
            LoadCommand = new RelayCommand(async _ => await LoadBatches());
            StartAnalysisCommand = new RelayCommand(async _ => await StartAnalysis(), _ => SelectedBatch != null && SelectedBatch.status == "pending");
            AddQualityControlCommand = new RelayCommand(_ => OnAddQualityControl?.Invoke(SelectedBatch), _ => SelectedBatch != null && (SelectedBatch.status == "in_analysis" || SelectedBatch.status == "pending"));
            ApproveCommand = new RelayCommand(async _ => await ApproveBatch(), _ => SelectedBatch != null && SelectedBatch.status == "in_analysis");
            BlockCommand = new RelayCommand(async _ => await BlockBatch(), _ => SelectedBatch != null && SelectedBatch.status == "in_analysis");
            ViewControlsCommand = new RelayCommand(_ => OnViewControls?.Invoke(SelectedBatch), _ => SelectedBatch != null);
        }

        public async Task LoadBatches()
        {
            try
            {
                var list = await ApiService.GetRawMaterialBatches();
                Batches.Clear();
                foreach (var b in list) Batches.Add(b);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки партий сырья: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task StartAnalysis()
        {
            try
            {
                SelectedBatch.status = "in_analysis";
                await ApiService.UpdateRawMaterialBatch(SelectedBatch.id, SelectedBatch);
                await LogAction("raw_material_batches", SelectedBatch.id, "update", null, $"статус изменён на in_analysis");
                await LoadBatches();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ApproveBatch()
        {
            try
            {
                // Проверим, есть ли хотя бы один контроль
                var controls = await ApiService.GetQualityControlsForRawMaterialBatch(SelectedBatch.id);
                if (controls.Count == 0)
                {
                    MessageBox.Show("Невозможно утвердить партию без результатов контроля.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                // Все контроли должны иметь result = "pass"
                if (controls.Any(qc => qc.result != "pass"))
                {
                    MessageBox.Show("Не все параметры контроля пройдены (есть fail). Партия не может быть одобрена.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                await ApiService.ApproveRawMaterialBatch(SelectedBatch.id);
                await LogAction("raw_material_batches", SelectedBatch.id, "approve", null, "партия одобрена");
                await LoadBatches();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task BlockBatch()
        {
            var comment = Microsoft.VisualBasic.Interaction.InputBox("Причина блокировки:", "Блокировка партии", "");
            if (string.IsNullOrWhiteSpace(comment))
            {
                MessageBox.Show("Необходимо указать причину блокировки.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                await ApiService.BlockRawMaterialBatch(SelectedBatch.id);
                await LogAction("raw_material_batches", SelectedBatch.id, "block", null, comment);
                await LoadBatches();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LogAction(string table, int recordId, string action, string oldVal, string newVal)
        {
            var log = new AuditLog
            {
                table_name = table,
                record_id = recordId,
                action = action,
                old_value = oldVal,
                new_value = newVal,
                changed_by = App.CurrentUser?.id,
                changed_at = DateTime.Now
            };
            try { await ApiService.CreateAuditLog(log); } catch { }
        }
    }
}