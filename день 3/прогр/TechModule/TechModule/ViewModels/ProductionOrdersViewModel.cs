using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TechModule.Models;
using TechModule.Services;

namespace TechModule.ViewModels
{
    public class ProductionOrdersViewModel : BaseViewModel
    {
        public ObservableCollection<ProductionOrder> Orders { get; } = new ObservableCollection<ProductionOrder>();
        private ProductionOrder _selectedOrder;
        public ProductionOrder SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                Set(ref _selectedOrder, value);
                ((RelayCommand)EditCommand)?.CanExecute(null);
                ((RelayCommand)DeleteCommand)?.CanExecute(null);
                ((RelayCommand)SetStatusCommand)?.CanExecute(null);
            }
        }

        public ICommand LoadCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SetStatusCommand { get; }

        public Action<ProductionOrder> OnEditOrder { get; set; }

        public ProductionOrdersViewModel()
        {
            LoadCommand = new RelayCommand(async _ => await LoadOrders());
            AddCommand = new RelayCommand(_ => OnEditOrder?.Invoke(null));
            EditCommand = new RelayCommand(_ => OnEditOrder?.Invoke(SelectedOrder), _ => SelectedOrder != null);
            DeleteCommand = new RelayCommand(async _ => await DeleteOrder(), _ => SelectedOrder != null);
            SetStatusCommand = new RelayCommand(async param => await SetStatus(param?.ToString()), _ => SelectedOrder != null);
        }

        public async Task LoadOrders()
        {
            try
            {
                var list = await ApiService.GetAsync<System.Collections.Generic.List<ProductionOrder>>("production_orders");
                Orders.Clear();
                foreach (var o in list) Orders.Add(o);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteOrder()
        {
            if (SelectedOrder == null) return;
            try
            {
                await ApiService.DeleteAsync($"production_orders/{SelectedOrder.id}");
                Orders.Remove(SelectedOrder);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SetStatus(string status)
        {
            if (string.IsNullOrEmpty(status)) return;
            try
            {
                var req = new { status = status };
                await ApiService.PostAsync<dynamic>($"production_orders/{SelectedOrder.id}/set-status", req);
                await LoadOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка смены статуса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}