using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using LabModule.Models;
using LabModule.Services;

namespace LabModule.ViewModels
{
    public class ProtocolViewModel : BaseViewModel
    {
        private string _protocolText;
        public string ProtocolText { get => _protocolText; set => Set(ref _protocolText, value); }

        public ICommand SaveCommand { get; }

        public ProtocolViewModel(int? rawMaterialBatchId, int? batchId)
        {
            SaveCommand = new RelayCommand(_ => SaveToFile());
            LoadAsync(rawMaterialBatchId, batchId);
        }

        private async void LoadAsync(int? rawMaterialBatchId, int? batchId)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("ПРОТОКОЛ КОНТРОЛЯ КАЧЕСТВА");
                sb.AppendLine($"Дата формирования: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine();

                if (rawMaterialBatchId.HasValue)
                {
                    var batch = await ApiService.GetRawMaterialBatch(rawMaterialBatchId.Value);
                    sb.AppendLine($"Партия сырья: {batch.batch_number}");
                    sb.AppendLine($"Поставщик: {batch.supplier}");
                    sb.AppendLine($"Дата поступления: {batch.received_date:yyyy-MM-dd}");
                    sb.AppendLine($"Количество: {batch.quantity} {batch.unit}");
                    sb.AppendLine($"Статус: {batch.status}");
                    sb.AppendLine();

                    var controls = await ApiService.GetQualityControlsForRawMaterialBatch(rawMaterialBatchId.Value);
                    sb.AppendLine("Результаты испытаний:");
                    foreach (var c in controls)
                    {
                        sb.AppendLine($"  Параметр: {c.parameter_name} ({c.unit})");
                        sb.AppendLine($"    Норма: {c.standard_value}");
                        sb.AppendLine($"    Измерено: {c.measured_value}");
                        sb.AppendLine($"    Результат: {(c.result == "pass" ? "Пройден" : "Не пройден")}");
                        if (!string.IsNullOrEmpty(c.decision))
                            sb.AppendLine($"    Решение: {c.decision} (комментарий: {c.analyst_comment})");
                        sb.AppendLine();
                    }
                }
                else if (batchId.HasValue)
                {
                    var batch = await ApiService.GetBatch(batchId.Value);
                    sb.AppendLine($"Партия готовой продукции: {batch.batch_number}");
                    sb.AppendLine($"Статус: {batch.status}");
                    sb.AppendLine($"Фактическое количество: {batch.actual_quantity_kg} кг");
                    sb.AppendLine();

                    var controls = await ApiService.GetQualityControlsForBatch(batchId.Value);
                    sb.AppendLine("Результаты испытаний:");
                    foreach (var c in controls)
                    {
                        sb.AppendLine($"  Параметр: {c.parameter_name} ({c.unit})");
                        sb.AppendLine($"    Норма: {c.standard_value}");
                        sb.AppendLine($"    Измерено: {c.measured_value}");
                        sb.AppendLine($"    Результат: {(c.result == "pass" ? "Пройден" : "Не пройден")}");
                        if (!string.IsNullOrEmpty(c.decision))
                            sb.AppendLine($"    Решение: {c.decision} (комментарий: {c.analyst_comment})");
                        sb.AppendLine();
                    }
                }

                ProtocolText = sb.ToString();
            }
            catch (Exception ex)
            {
                ProtocolText = $"Ошибка формирования протокола: {ex.Message}";
            }
        }

        private void SaveToFile()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                DefaultExt = ".txt",
                FileName = $"Protocol_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };
            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, ProtocolText, Encoding.UTF8);
                MessageBox.Show("Протокол сохранён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}