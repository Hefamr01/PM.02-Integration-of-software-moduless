using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using OperatorModule.Models;
using OperatorModule.Services;

namespace OperatorModule.ViewModels
{
    public class BatchExecutionViewModel : BaseViewModel
    {
        private Batch _batch;
        public Batch Batch
        {
            get => _batch;
            set => Set(ref _batch, value);
        }

        public ObservableCollection<BatchStep> Steps { get; } = new ObservableCollection<BatchStep>();
        private BatchStep _currentStep;
        public BatchStep CurrentStep
        {
            get => _currentStep;
            set
            {
                Set(ref _currentStep, value);
                if (value != null)
                {
                    ActualTemp = value.actual_temp_c ?? value.planned_temp_c ?? 0;
                    ActualPressure = value.actual_pressure_bar ?? value.planned_pressure_bar ?? 0;
                    ActualDuration = value.actual_duration_min ?? 0;
                    OperatorComment = value.operator_comment ?? "";
                    StartTelemetry();
                }
                else
                {
                    StopTelemetry();
                }
            }
        }

        private string _operatorComment;
        public string OperatorComment
        {
            get => _operatorComment;
            set => Set(ref _operatorComment, value);
        }

        private decimal _actualTemp;
        public decimal ActualTemp
        {
            get => _actualTemp;
            set => Set(ref _actualTemp, value);
        }

        private decimal _actualPressure;
        public decimal ActualPressure
        {
            get => _actualPressure;
            set => Set(ref _actualPressure, value);
        }

        private int _actualDuration;
        public int ActualDuration
        {
            get => _actualDuration;
            set => Set(ref _actualDuration, value);
        }

        // Телеметрия
        private decimal _telemetryTemp;
        public decimal TelemetryTemp
        {
            get => _telemetryTemp;
            set => Set(ref _telemetryTemp, value);
        }

        private decimal _telemetryPressure;
        public decimal TelemetryPressure
        {
            get => _telemetryPressure;
            set => Set(ref _telemetryPressure, value);
        }

        private bool _isTelemetryRunning;
        private DispatcherTimer _telemetryTimer;
        private Random _random = new Random();

        public ICommand LoadCommand { get; }
        public ICommand StartStepCommand { get; }
        public ICommand FinishStepCommand { get; }
        public ICommand CompleteBatchCommand { get; }
        public Action OnBatchCompleted { get; set; }

        public BatchExecutionViewModel(Batch batch)
        {
            Batch = batch;
            LoadCommand = new RelayCommand(async _ => await LoadSteps());
            StartStepCommand = new RelayCommand(async _ => await StartCurrentStep(), _ => CurrentStep != null && !CurrentStep.started_at.HasValue);
            FinishStepCommand = new RelayCommand(async _ => await FinishCurrentStep(), _ => CurrentStep != null && CurrentStep.started_at.HasValue && !CurrentStep.completed_at.HasValue);
            CompleteBatchCommand = new RelayCommand(async _ => await CompleteBatch(), _ => Steps.All(s => s.completed_at.HasValue));
        }

        public async Task LoadSteps()
        {
            try
            {
                var details = await ApiService.GetBatchDetails(Batch.id);
                Batch = new Batch
                {
                    id = details.batch.id,
                    batch_number = details.batch.batch_number,
                    status = details.batch.status,
                    actual_quantity_kg = details.batch.actual_quantity_kg
                };
                var steps = details.steps.ToObject<BatchStep[]>();
                Steps.Clear();
                foreach (var s in new List<dynamic>(steps).OrderBy(x => x.step_order))
                    Steps.Add(s);

                // Определяем текущий шаг: первый незавершённый
                CurrentStep = Steps.FirstOrDefault(s => !s.completed_at.HasValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки шагов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Только изменённые методы: StartCurrentStep, FinishCurrentStep (добавлены проверки)
        // Остальной код остаётся без изменений

        private async Task StartCurrentStep()
        {
            // Проверка, не начат ли уже шаг
            if (CurrentStep == null) return;
            if (CurrentStep.started_at.HasValue)
            {
                MessageBox.Show("Шаг уже начат.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var step = await ApiService.StartBatchStep(CurrentStep.id, App.CurrentUser.id);
                CurrentStep.started_at = step.started_at;
                CurrentStep.started_by = step.started_by;
                OnPropertyChanged(nameof(CurrentStep));
                MessageBox.Show($"Шаг '{CurrentStep.step_name}' начат.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка начала шага: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task FinishCurrentStep()
        {
            // Проверка, не завершён ли уже шаг
            if (CurrentStep == null) return;
            if (CurrentStep.completed_at.HasValue)
            {
                MessageBox.Show("Шаг уже завершён.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Валидация обязательности ввода (если шаг обязателен, то все поля должны быть заполнены? по заданию - проверки перед сохранением)
            if (CurrentStep.planned_temp_c.HasValue && ActualTemp == 0)
            {
                var result = MessageBox.Show("Температура не указана. Продолжить?", "Внимание",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes) return;
            }
            if (CurrentStep.planned_pressure_bar.HasValue && ActualPressure == 0)
            {
                var result = MessageBox.Show("Давление не указано. Продолжить?", "Внимание",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes) return;
            }
            if (CurrentStep.planned_duration_min.HasValue && ActualDuration == 0)
            {
                var result = MessageBox.Show("Длительность не указана. Продолжить?", "Внимание",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes) return;
            }

            // Дополнительная проверка на допустимость значений (например, неотрицательность)
            if (ActualTemp < 0)
            {
                MessageBox.Show("Температура не может быть отрицательной.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (ActualPressure < 0)
            {
                MessageBox.Show("Давление не может быть отрицательным.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (ActualDuration < 0)
            {
                MessageBox.Show("Длительность не может быть отрицательной.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Ограничения по длине/диапазону (примеры)
            if (ActualTemp > 1000)
            {
                MessageBox.Show("Температура слишком высокая (макс. 1000°C).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (ActualPressure > 100)
            {
                MessageBox.Show("Давление слишком высокое (макс. 100 бар).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (ActualDuration > 10080) // неделя в минутах
            {
                MessageBox.Show("Длительность не может превышать 10080 минут (7 дней).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка отклонений (как было ранее)
            bool deviation = false;
            string deviationMsg = "";
            if (CurrentStep.planned_temp_c.HasValue)
            {
                var tolerance = CurrentStep.planned_temp_c.Value * 0.05m;
                if (ActualTemp < CurrentStep.planned_temp_c.Value - tolerance || ActualTemp > CurrentStep.planned_temp_c.Value + tolerance)
                {
                    deviation = true;
                    deviationMsg += $"Температура выходит за пределы ±5% (план: {CurrentStep.planned_temp_c}°C). ";
                }
            }
            if (CurrentStep.planned_pressure_bar.HasValue)
            {
                var tolerance = CurrentStep.planned_pressure_bar.Value * 0.05m;
                if (ActualPressure < CurrentStep.planned_pressure_bar.Value - tolerance || ActualPressure > CurrentStep.planned_pressure_bar.Value + tolerance)
                {
                    deviation = true;
                    deviationMsg += $"Давление выходит за пределы ±5% (план: {CurrentStep.planned_pressure_bar} бар). ";
                }
            }

            if (deviation)
            {
                var confirm = MessageBox.Show($"{deviationMsg}\nВыявлено отклонение. Продолжить?", "Отклонение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm != MessageBoxResult.Yes) return;
            }

            try
            {
                string comment = OperatorComment;
                if (deviation && string.IsNullOrWhiteSpace(comment))
                {
                    comment = "Отклонение зафиксировано оператором";
                }

                var finishedStep = await ApiService.FinishBatchStep(CurrentStep.id, App.CurrentUser.id,
                    ActualTemp, ActualPressure, ActualDuration, comment);

                if (deviation && !finishedStep.deviation_flag)
                {
                    finishedStep.deviation_flag = true;
                    await ApiService.UpdateBatchStep(CurrentStep.id, finishedStep);
                }

                CurrentStep.completed_at = finishedStep.completed_at;
                CurrentStep.actual_temp_c = finishedStep.actual_temp_c;
                CurrentStep.actual_pressure_bar = finishedStep.actual_pressure_bar;
                CurrentStep.actual_duration_min = finishedStep.actual_duration_min;
                CurrentStep.deviation_flag = finishedStep.deviation_flag;
                CurrentStep.operator_comment = finishedStep.operator_comment;
                OnPropertyChanged(nameof(CurrentStep));

                CurrentStep = Steps.FirstOrDefault(s => !s.completed_at.HasValue);
                if (CurrentStep == null)
                {
                    MessageBox.Show("Все шаги выполнены. Завершите партию.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка завершения шага: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CompleteBatch()
        {
            try
            {
                var completedBatch = await ApiService.CompleteBatch(Batch.id, Batch.actual_quantity_kg);
                MessageBox.Show($"Партия {completedBatch.batch_number} завершена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                OnBatchCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка завершения партии: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartTelemetry()
        {
            if (_isTelemetryRunning) return;
            if (CurrentStep == null) return;
            if (!CurrentStep.planned_temp_c.HasValue && !CurrentStep.planned_pressure_bar.HasValue) return;

            _telemetryTimer = new DispatcherTimer();
            _telemetryTimer.Interval = TimeSpan.FromSeconds(1);
            _telemetryTimer.Tick += (s, e) => UpdateTelemetry();
            _telemetryTimer.Start();
            _isTelemetryRunning = true;
        }

        private void StopTelemetry()
        {
            if (_telemetryTimer != null)
            {
                _telemetryTimer.Stop();
                _telemetryTimer = null;
            }
            _isTelemetryRunning = false;
        }

        private void UpdateTelemetry()
        {
            if (CurrentStep == null) return;
            if (CurrentStep.planned_temp_c.HasValue)
            {
                var baseTemp = CurrentStep.planned_temp_c.Value;
                var variation = (decimal)(_random.NextDouble() * 0.1 - 0.05); // -5%..+5%
                TelemetryTemp = baseTemp + baseTemp * variation;
            }
            if (CurrentStep.planned_pressure_bar.HasValue)
            {
                var basePress = CurrentStep.planned_pressure_bar.Value;
                var variation = (decimal)(_random.NextDouble() * 0.1 - 0.05);
                TelemetryPressure = basePress + basePress * variation;
            }
        }
    }
}