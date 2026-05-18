using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TechModule.Models;
using TechModule.Services;

namespace TechModule.ViewModels
{
    public class TechMapEditViewModel : BaseViewModel
    {
        public TechMap Map { get; set; } = new TechMap();
        public ObservableCollection<TechMapStep> Steps { get; } = new ObservableCollection<TechMapStep>();
        private TechMapStep _selectedStep;
        public TechMapStep SelectedStep
        {
            get => _selectedStep;
            set
            {
                Set(ref _selectedStep, value);
                ((RelayCommand)RemoveStepCommand)?.CanExecute(null);
            }
        }

        public bool IsNew => Map.id == 0;
        private string _error;
        public string Error { get => _error; set => Set(ref _error, value); }

        // Свойства для добавления/редактирования шага
        private int _stepOrder;
        public int StepOrder { get => _stepOrder; set => Set(ref _stepOrder, value); }
        private string _stepName;
        public string StepName { get => _stepName; set => Set(ref _stepName, value); }
        private string _stepType;
        public string StepType { get => _stepType; set => Set(ref _stepType, value); }
        private decimal? _plannedTemp;
        public decimal? PlannedTemp { get => _plannedTemp; set => Set(ref _plannedTemp, value); }
        private decimal? _plannedPressure;
        public decimal? PlannedPressure { get => _plannedPressure; set => Set(ref _plannedPressure, value); }
        private int? _plannedDuration;
        public int? PlannedDuration { get => _plannedDuration; set => Set(ref _plannedDuration, value); }
        private bool _isMandatory;
        public bool IsMandatory { get => _isMandatory; set => Set(ref _isMandatory, value); }
        private string _instruction;
        public string Instruction { get => _instruction; set => Set(ref _instruction, value); }

        public ICommand SaveCommand { get; }
        public ICommand AddStepCommand { get; }
        public ICommand RemoveStepCommand { get; }
        public ICommand EditStepCommand { get; }
        public Action OnSaved { get; set; }

        public TechMapEditViewModel(TechMap map = null)
        {
            if (map != null)
            {
                Map = new TechMap
                {
                    id = map.id,
                    product_id = map.product_id,
                    version = map.version,
                    status = map.status,
                    created_at = map.created_at,
                    created_by = map.created_by
                };
            }
            SaveCommand = new RelayCommand(async _ => await Save());
            AddStepCommand = new RelayCommand(async _ => await AddStep());
            RemoveStepCommand = new RelayCommand(async _ => await RemoveStep(), _ => SelectedStep != null);
            EditStepCommand = new RelayCommand(_ => LoadStepForEdit(SelectedStep), _ => SelectedStep != null);
        }

        public async Task InitializeAsync()
        {
            if (Map.id != 0)
                await LoadSteps();
        }

        private async Task LoadSteps()
        {
            if (Map.id == 0) return;
            try
            {
                var steps = await ApiService.GetAsync<System.Collections.Generic.List<TechMapStep>>($"tech-map-steps/by-map/{Map.id}");
                Steps.Clear();
                foreach (var s in steps) Steps.Add(s);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки шагов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStepForEdit(TechMapStep step)
        {
            if (step == null) return;
            StepOrder = step.step_order;
            StepName = step.step_name;
            StepType = step.step_type;
            PlannedTemp = step.planned_temp_c;
            PlannedPressure = step.planned_pressure_bar;
            PlannedDuration = step.planned_duration_min;
            IsMandatory = step.is_mandatory;
            Instruction = step.instruction;
            Steps.Remove(step);
            SelectedStep = null;
        }

        private async Task AddStep()
        {
            if (string.IsNullOrWhiteSpace(StepName))
            {
                Error = "Введите название шага";
                return;
            }
            var newStep = new TechMapStep
            {
                tech_map_id = Map.id,
                step_order = StepOrder,
                step_name = StepName,
                step_type = StepType,
                planned_temp_c = PlannedTemp,
                planned_pressure_bar = PlannedPressure,
                planned_duration_min = PlannedDuration,
                is_mandatory = IsMandatory,
                instruction = Instruction
            };

            try
            {
                if (Map.id != 0)
                {
                    var added = await ApiService.PostAsync<TechMapStep>("tech_map_steps", newStep);
                    Steps.Add(added);
                }
                else
                {
                    Steps.Add(newStep);
                }
                ClearStepFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления шага: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RemoveStep()
        {
            try
            {
                if (SelectedStep.id != 0)
                    await ApiService.DeleteAsync($"tech_map_steps/{SelectedStep.id}");
                Steps.Remove(SelectedStep);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления шага: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearStepFields()
        {
            StepOrder = Steps.Count + 1;
            StepName = "";
            StepType = "";
            PlannedTemp = null;
            PlannedPressure = null;
            PlannedDuration = null;
            IsMandatory = false;
            Instruction = "";
        }

        private async Task Save()
        {
            Error = string.Empty;
            try
            {
                if (IsNew)
                {
                    Map.created_by = App.CurrentUser?.id ?? 1;
                    var created = await ApiService.PostAsync<TechMap>("tech_maps", Map);
                    Map.id = created.id;
                }
                else
                {
                    await ApiService.PutAsync($"tech_maps/{Map.id}", Map);
                }
                // Сохранить шаги, у которых id == 0
                foreach (var step in Steps.Where(s => s.id == 0))
                {
                    step.tech_map_id = Map.id;
                    await ApiService.PostAsync<TechMapStep>("tech_map_steps", step);
                }
                OnSaved?.Invoke();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                MessageBox.Show(Error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}