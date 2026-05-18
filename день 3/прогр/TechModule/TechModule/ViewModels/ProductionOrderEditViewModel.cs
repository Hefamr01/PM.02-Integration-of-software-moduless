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
    public class ProductionOrderEditViewModel : BaseViewModel
    {
        public ProductionOrder Order { get; set; } = new ProductionOrder();
        public ObservableCollection<Recipe> Recipes { get; } = new ObservableCollection<Recipe>();
        private Recipe _selectedRecipe;
        public Recipe SelectedRecipe
        {
            get => _selectedRecipe;
            set
            {
                if (Set(ref _selectedRecipe, value))
                    Order.recipe_id = value?.id ?? 0;
            }
        }

        public bool IsNew => Order.id == 0;
        private string _error;
        public string Error { get => _error; set => Set(ref _error, value); }

        public ICommand SaveCommand { get; }
        public Action OnSaved { get; set; }

        public ProductionOrderEditViewModel(ProductionOrder order = null)
        {
            if (order != null)
            {
                Order = new ProductionOrder
                {
                    id = order.id,
                    order_number = order.order_number,
                    recipe_id = order.recipe_id,
                    planned_quantity_kg = order.planned_quantity_kg,
                    status = order.status,
                    planned_start_date = order.planned_start_date,
                    created_by = order.created_by
                };
            }
            SaveCommand = new RelayCommand(async _ => await Save());
        }

        public async Task InitializeAsync()
        {
            await LoadRecipes();
            if (Order.recipe_id != 0)
                SelectedRecipe = Recipes.FirstOrDefault(r => r.id == Order.recipe_id);
        }

        private async Task LoadRecipes()
        {
            try
            {
                var list = await ApiService.GetAsync<System.Collections.Generic.List<Recipe>>("recipes");
                Recipes.Clear();
                foreach (var r in list) Recipes.Add(r);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки рецептов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task Save()
        {
            Error = string.Empty;
            try
            {
                if (IsNew)
                {
                    Order.created_by = App.CurrentUser?.id ?? 1;
                    var created = await ApiService.PostAsync<ProductionOrder>("production_orders", Order);
                    Order.id = created.id;
                }
                else
                {
                    await ApiService.PutAsync($"production_orders/{Order.id}", Order);
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