using System.Windows.Controls;
using TechModule.ViewModels;

namespace TechModule.Views.Pages
{
    public partial class RecipesPage : Page
    {
        private RecipesViewModel _vm;
        public RecipesPage()
        {
            InitializeComponent();
            _vm = new RecipesViewModel();
            _vm.OnEditRecipe = recipe =>
            {
                var editPage = new RecipeEditPage(recipe);
                NavigationService.Navigate(editPage);
            };
            DataContext = _vm;
            Loaded += async (s, e) => await _vm.LoadRecipes();
        }
    }
}