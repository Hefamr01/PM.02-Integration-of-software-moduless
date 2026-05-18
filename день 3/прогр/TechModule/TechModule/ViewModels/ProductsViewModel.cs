using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TechModule.Models;
using TechModule.Services;

namespace TechModule.ViewModels
{
    public class ProductsViewModel : BaseViewModel
    {
        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                Set(ref _selectedProduct, value);
                ((RelayCommand)EditCommand)?.CanExecute(null);
            }
        }

        public ICommand LoadCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }
        public Action<Product> OnEditProduct { get; set; }

        public ProductsViewModel()
        {
            LoadCommand = new RelayCommand(async _ => await LoadProducts());
            AddCommand = new RelayCommand(_ => OnEditProduct?.Invoke(null));
            EditCommand = new RelayCommand(_ => OnEditProduct?.Invoke(SelectedProduct), _ => SelectedProduct != null);
            DeleteCommand = new RelayCommand(async _ => await DeleteProduct(), _ => SelectedProduct != null);
            RefreshCommand = new RelayCommand(async _ => await LoadProducts());
        }

        public async Task LoadProducts()
        {
            try
            {
                var list = await ApiService.GetAsync<System.Collections.Generic.List<Product>>("products");
                Products.Clear();
                foreach (var p in list) Products.Add(p);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продуктов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteProduct()
        {
            if (SelectedProduct == null) return;
            try
            {
                await ApiService.DeleteAsync($"products/{SelectedProduct.id}");
                Products.Remove(SelectedProduct);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}