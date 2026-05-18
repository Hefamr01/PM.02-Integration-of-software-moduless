using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LabModule.Models;
using LabModule.Services;

namespace LabModule.ViewModels
{
    public class QualityControlEditViewModel : BaseViewModel
    {
        public QualityControl Control { get; set; } = new QualityControl();
        public bool IsNew => Control.id == 0;
        public bool IsForRawMaterial { get; set; }

        private string _error;
        public string Error { get => _error; set => Set(ref _error, value); }

        public ICommand SaveCommand { get; }
        public Action OnSaved { get; set; }

        public QualityControlEditViewModel(QualityControl control = null, bool forRawMaterial = true)
        {
            IsForRawMaterial = forRawMaterial;
            if (control != null)
            {
                Control = new QualityControl
                {
                    id = control.id,
                    batch_id = control.batch_id,
                    raw_material_batch_id = control.raw_material_batch_id,
                    analysis_date = control.analysis_date,
                    sample_type = control.sample_type,
                    parameter_name = control.parameter_name,
                    measured_value = control.measured_value,
                    standard_value = control.standard_value,
                    unit = control.unit,
                    result = control.result,
                    decision = control.decision,
                    analyst_id = control.analyst_id,
                    analyst_comment = control.analyst_comment
                };
            }
            else
            {
                Control.analysis_date = DateTime.Now;
                Control.analyst_id = App.CurrentUser?.id;
                Control.sample_type = forRawMaterial ? "raw_material" : "finished_product";
            }
            SaveCommand = new RelayCommand(async _ => await Save());
        }

        private async Task Save()
        {
            Error = string.Empty;
            try
            {
                // Автоматическая проверка соответствия стандарту
                if (!string.IsNullOrWhiteSpace(Control.standard_value) && Control.measured_value.HasValue)
                {
                    Control.result = EvaluateResult(Control.measured_value.Value, Control.standard_value);
                }
                else
                {
                    Control.result = null; // не определен
                }

                if (IsNew)
                {
                    var created = await ApiService.CreateQualityControl(Control);
                    Control.id = created.id;
                }
                else
                {
                    await ApiService.PutAsync($"quality_controls/{Control.id}", Control);
                }
                OnSaved?.Invoke();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                MessageBox.Show(Error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string EvaluateResult(decimal measured, string standard)
        {
            // Поддержка форматов: "min;max" или ">=min" или "<=max"
            if (standard.Contains(";"))
            {
                var parts = standard.Split(';');
                if (parts.Length == 2 &&
                    decimal.TryParse(parts[0], out decimal min) &&
                    decimal.TryParse(parts[1], out decimal max))
                {
                    return (measured >= min && measured <= max) ? "pass" : "fail";
                }
            }
            else if (standard.StartsWith(">="))
            {
                if (decimal.TryParse(standard.Substring(2), out decimal min))
                    return measured >= min ? "pass" : "fail";
            }
            else if (standard.StartsWith("<="))
            {
                if (decimal.TryParse(standard.Substring(2), out decimal max))
                    return measured <= max ? "pass" : "fail";
            }
            else if (decimal.TryParse(standard, out decimal exact))
            {
                return measured == exact ? "pass" : "fail";
            }
            return "fail";
        }
    }
}