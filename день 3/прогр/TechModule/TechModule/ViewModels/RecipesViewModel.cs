using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TechModule.Models;
using TechModule.Services;

namespace TechModule.ViewModels
{
    public class RecipesViewModel : BaseViewModel
    {
        public ObservableCollection<Recipe> Recipes { get; } = new ObservableCollection<Recipe>();
        private Recipe _selectedRecipe;
        public Recipe SelectedRecipe
        {
            get => _selectedRecipe;
            set { Set(ref _selectedRecipe, value); ((RelayCommand)EditCommand)?.CanExecute(null); ((RelayCommand)ActivateCommand)?.CanExecute(null); ((RelayCommand)ArchiveCommand)?.CanExecute(null); }
        }

        public ICommand LoadCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ActivateCommand { get; }
        public ICommand ArchiveCommand { get; }

        public Action<Recipe> OnEditRecipe { get; set; }

        public RecipesViewModel()
        {
            LoadCommand = new RelayCommand(async _ => await LoadRecipes());
            AddCommand = new RelayCommand(_ => OnEditRecipe?.Invoke(null));
            EditCommand = new RelayCommand(_ => OnEditRecipe?.Invoke(SelectedRecipe), _ => SelectedRecipe != null);
            DeleteCommand = new RelayCommand(async _ => await DeleteRecipe(), _ => SelectedRecipe != null);
            ActivateCommand = new RelayCommand(async _ => await ActivateRecipe(), _ => SelectedRecipe != null && SelectedRecipe.status != "active");
            ArchiveCommand = new RelayCommand(async _ => await ArchiveRecipe(), _ => SelectedRecipe != null && SelectedRecipe.status != "archived");
        }

        public async Task LoadRecipes()
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

        private async Task DeleteRecipe()
        {
            try
            {
                await ApiService.DeleteAsync($"recipes/{SelectedRecipe.id}");
                Recipes.Remove(SelectedRecipe);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ActivateRecipe()
        {
            try
            {
                await ApiService.ActivateRecipeAsync<Recipe>(SelectedRecipe.id);
                await LoadRecipes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка активации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ArchiveRecipe()
        {
            try
            {
                await ApiService.ArchiveRecipeAsync<Recipe>(SelectedRecipe.id);
                await LoadRecipes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка архивации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}