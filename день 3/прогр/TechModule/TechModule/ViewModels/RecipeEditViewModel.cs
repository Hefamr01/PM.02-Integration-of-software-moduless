using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using TechModule.Models;
using TechModule.Services;

namespace TechModule.ViewModels
{
    public class RecipeEditViewModel : BaseViewModel
    {
        public Recipe Recipe { get; set; } = new Recipe();
        public ObservableCollection<RecipeComponent> Components { get; } = new ObservableCollection<RecipeComponent>();
        public ObservableCollection<RawMaterial> RawMaterials { get; } = new ObservableCollection<RawMaterial>();

        private RecipeComponent _selectedComponent;
        public RecipeComponent SelectedComponent
        {
            get => _selectedComponent;
            set { Set(ref _selectedComponent, value); ((RelayCommand)RemoveComponentCommand)?.CanExecute(null); }
        }

        public bool IsNew => Recipe.id == 0;
        private string _error;
        public string Error { get => _error; set => Set(ref _error, value); }

        // Поля для добавления компонента
        private int _selectedRawMaterialId;
        public int SelectedRawMaterialId
        {
            get => _selectedRawMaterialId;
            set => Set(ref _selectedRawMaterialId, value);
        }
        private decimal _percentage;
        public decimal Percentage
        {
            get => _percentage;
            set => Set(ref _percentage, value);
        }
        private int _loadOrder;
        public int LoadOrder
        {
            get => _loadOrder;
            set => Set(ref _loadOrder, value);
        }
        private decimal _toleranceMin;
        public decimal ToleranceMin { get => _toleranceMin; set => Set(ref _toleranceMin, value); }
        private decimal _toleranceMax;
        public decimal ToleranceMax { get => _toleranceMax; set => Set(ref _toleranceMax, value); }

        public ICommand SaveCommand { get; }
        public ICommand AddComponentCommand { get; }
        public ICommand RemoveComponentCommand { get; }
        public Action OnSaved { get; set; }

        public RecipeEditViewModel(Recipe recipe = null)
        {
            if (recipe != null)
            {
                Recipe = new Recipe
                {
                    id = recipe.id,
                    product_id = recipe.product_id,
                    version = recipe.version,
                    status = recipe.status,
                    created_at = recipe.created_at,
                    approved_at = recipe.approved_at,
                    created_by = recipe.created_by
                };
            }
            SaveCommand = new RelayCommand(async _ => await Save());
            AddComponentCommand = new RelayCommand(async _ => await AddComponent());
            RemoveComponentCommand = new RelayCommand(async _ => await RemoveComponent(), _ => SelectedComponent != null);
        }

        public async Task InitializeAsync()
        {
            await LoadRawMaterials();
            if (Recipe.id != 0)
                await LoadComponents();
        }

        private async Task LoadRawMaterials()
        {
            try
            {
                var list = await ApiService.GetAsync<System.Collections.Generic.List<RawMaterial>>("raw_materials");
                RawMaterials.Clear();
                foreach (var rm in list) RawMaterials.Add(rm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сырья: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadComponents()
        {
            if (Recipe.id == 0) return;
            try
            {
                var comps = await ApiService.GetAsync<System.Collections.Generic.List<RecipeComponent>>($"recipe-components/by-recipe/{Recipe.id}");
                Components.Clear();
                foreach (var c in comps) Components.Add(c);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки компонентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddComponent()
        {
            if (SelectedRawMaterialId == 0) { Error = "Выберите сырьё"; return; }
            var newComp = new RecipeComponent
            {
                recipe_id = Recipe.id,
                raw_material_id = SelectedRawMaterialId,
                percentage = Percentage,
                load_order = LoadOrder,
                tolerance_min = ToleranceMin,
                tolerance_max = ToleranceMax
            };
            try
            {
                if (Recipe.id != 0)
                {
                    var added = await ApiService.PostAsync<RecipeComponent>("recipe_components", newComp);
                    Components.Add(added);
                }
                else
                {
                    Components.Add(newComp);
                }
                // Сброс полей
                SelectedRawMaterialId = 0;
                Percentage = 0;
                LoadOrder = Components.Count + 1;
                ToleranceMin = 0;
                ToleranceMax = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления компонента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RemoveComponent()
        {
            try
            {
                if (SelectedComponent.id != 0)
                    await ApiService.DeleteAsync($"recipe_components/{SelectedComponent.id}");
                Components.Remove(SelectedComponent);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления компонента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task Save()
        {
            Error = string.Empty;
            try
            {
                Recipe createdRecipe = null;
                if (IsNew)
                {
                    createdRecipe = await ApiService.PostAsync<Recipe>("recipes", Recipe);
                    Recipe.id = createdRecipe.id;
                }
                else
                {
                    await ApiService.PutAsync($"recipes/{Recipe.id}", Recipe);
                    createdRecipe = Recipe;
                }
                // Сохраняем компоненты, которые ещё не отправлены (id == 0)
                foreach (var comp in Components.Where(c => c.id == 0))
                {
                    comp.recipe_id = createdRecipe.id;
                    await ApiService.PostAsync<RecipeComponent>("recipe_components", comp);
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