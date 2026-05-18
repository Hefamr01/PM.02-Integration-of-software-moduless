using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TechModule.Models;
using TechModule.Services;

namespace TechModule.ViewModels
{
    public class ProductEditViewModel : BaseViewModel
    {
        public Product Product { get; set; } = new Product();
        public bool IsNew => Product.id == 0;

        private string _error;
        public string Error { get => _error; set => Set(ref _error, value); }

        public ICommand SaveCommand { get; }
        public Action OnSaved { get; set; }

        public ProductEditViewModel(Product product = null)
        {
            if (product != null)
            {
                Product = new Product
                {
                    id = product.id,
                    name = product.name,
                    type = product.type,
                    form = product.form,
                    status = product.status
                };
            }
            SaveCommand = new RelayCommand(async _ => await Save());
        }

        private async Task Save()
        {
            Error = string.Empty;
            try
            {
                if (IsNew)
                {
                    var created = await ApiService.PostAsync<Product>("products", Product);
                    Product.id = created.id;
                }
                else
                {
                    await ApiService.PutAsync($"products/{Product.id}", Product);
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