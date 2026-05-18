using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TechModule.Models;
using TechModule.Services;

namespace TechModule.ViewModels
{
    public class BatchDetailsViewModel : BaseViewModel
    {
        public Batch Batch { get; set; }
        public ObservableCollection<BatchStep> Steps { get; } = new ObservableCollection<BatchStep>();
        public ObservableCollection<object> QualityControls { get; } = new ObservableCollection<object>();

        private BatchStep _selectedStep;
        public BatchStep SelectedStep
        {
            get => _selectedStep;
            set
            {
                Set(ref _selectedStep, value);
                ((RelayCommand)StartStepCommand)?.CanExecute(null);
                ((RelayCommand)FinishStepCommand)?.CanExecute(null);
            }
        }

        public ICommand LoadCommand { get; }
        public ICommand StartStepCommand { get; }
        public ICommand FinishStepCommand { get; }

        public BatchDetailsViewModel(Batch batch)
        {
            Batch = batch;
            LoadCommand = new RelayCommand(async _ => await LoadDetails());
            StartStepCommand = new RelayCommand(async _ => await StartStep(), _ => SelectedStep != null && SelectedStep.started_at == null);
            FinishStepCommand = new RelayCommand(async _ => await ShowFinishStepDialog(), _ => SelectedStep != null && SelectedStep.started_at != null && SelectedStep.completed_at == null);
        }

        public async Task LoadDetails()
        {
            try
            {
                var data = await ApiService.GetBatchDetailsAsync(Batch.id);
                Batch = data.batch;
                Steps.Clear();
                foreach (var step in data.steps) Steps.Add(step);
                QualityControls.Clear();
                if (data.controls != null)
                    foreach (var qc in data.controls) QualityControls.Add(qc);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки деталей партии: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task StartStep()
        {
            try
            {
                int userId = App.CurrentUser?.id ?? 1;
                await ApiService.StartBatchStepAsync<BatchStep>(SelectedStep.id, userId);
                await LoadDetails();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка старта шага: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ShowFinishStepDialog()
        {
            var dialog = new FinishStepDialog(SelectedStep);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    int userId = App.CurrentUser?.id ?? 1;
                    await ApiService.FinishBatchStepAsync<BatchStep>(
                        SelectedStep.id, userId,
                        dialog.ActualTemp, dialog.ActualPressure,
                        dialog.ActualDuration, dialog.OperatorComment);
                    await LoadDetails();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка завершения шага: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    public class FinishStepDialog : Window
    {
        public decimal? ActualTemp { get; set; }
        public decimal? ActualPressure { get; set; }
        public int? ActualDuration { get; set; }
        public string OperatorComment { get; set; }

        public FinishStepDialog(BatchStep step)
        {
            Title = "Завершение шага";
            Width = 400;
            Height = 300;
            var stack = new StackPanel { Margin = new Thickness(10) };

            stack.Children.Add(new TextBlock { Text = $"Шаг: {step.step_name}" });
            stack.Children.Add(new TextBlock { Text = "Фактическая температура (°C):" });
            var tempBox = new TextBox();
            stack.Children.Add(tempBox);
            stack.Children.Add(new TextBlock { Text = "Фактическое давление (бар):" });
            var pressBox = new TextBox();
            stack.Children.Add(pressBox);
            stack.Children.Add(new TextBlock { Text = "Фактическая длительность (мин):" });
            var durBox = new TextBox();
            stack.Children.Add(durBox);
            stack.Children.Add(new TextBlock { Text = "Комментарий оператора:" });
            var commentBox = new TextBox { Height = 60, TextWrapping = TextWrapping.Wrap };
            stack.Children.Add(commentBox);

            var saveBtn = new Button { Content = "Сохранить", Margin = new Thickness(0, 10, 0, 0) };
            saveBtn.Click += (s, e) =>
            {
                ActualTemp = decimal.TryParse(tempBox.Text, out var t) ? t : (decimal?)null;
                ActualPressure = decimal.TryParse(pressBox.Text, out var p) ? p : (decimal?)null;
                ActualDuration = int.TryParse(durBox.Text, out var d) ? d : (int?)null;
                OperatorComment = commentBox.Text;
                DialogResult = true;
                Close();
            };
            stack.Children.Add(saveBtn);
            Content = stack;
        }
    }
}