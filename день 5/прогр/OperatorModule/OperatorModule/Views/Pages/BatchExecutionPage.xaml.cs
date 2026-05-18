using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using OperatorModule.Models;
using OperatorModule.ViewModels;

namespace OperatorModule.Views.Pages
{
    public partial class BatchExecutionPage : Page
    {
        private BatchExecutionViewModel _vm;
        public System.Action OnBatchCompleted { get; set; }

        public BatchExecutionPage(Batch batch)
        {
            InitializeComponent();
            _vm = new BatchExecutionViewModel(batch);
            _vm.OnBatchCompleted = () => OnBatchCompleted?.Invoke();
            DataContext = _vm;
            Loaded += async (s, e) => await _vm.LoadSteps();
        }

        // Фильтрация ввода для вещественных чисел (с точкой или запятой)
        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            // Разрешаем: цифры, точка, запятая, минус (только в начале)
            // Также не даём ввести более одной точки/запятой
            string pattern = @"^[+-]?\d*[.,]?\d*$";
            if (!Regex.IsMatch(newText, pattern))
            {
                e.Handled = true;
                return;
            }

            // Проверка на количество десятичных разделителей
            int dotCount = newText.Count(c => c == '.' || c == ',');
            if (dotCount > 1)
            {
                e.Handled = true;
            }
        }

        private void OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pastedText = (string)e.DataObject.GetData(typeof(string));
                string newText = ((TextBox)sender).Text.Insert(((TextBox)sender).SelectionStart, pastedText);
                if (!Regex.IsMatch(newText, @"^[+-]?\d*[.,]?\d*$") || newText.Count(c => c == '.' || c == ',') > 1)
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        // Фильтрация для целых чисел (длительность)
        private void OnPreviewIntegerTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            if (!Regex.IsMatch(newText, @"^\d*$"))
            {
                e.Handled = true;
            }
        }

        private void OnPastingInteger(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pastedText = (string)e.DataObject.GetData(typeof(string));
                string newText = ((TextBox)sender).Text.Insert(((TextBox)sender).SelectionStart, pastedText);
                if (!Regex.IsMatch(newText, @"^\d*$"))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}