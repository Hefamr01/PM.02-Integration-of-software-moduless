using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TechModule.Models;
using TechModule.Services;

namespace TechModule.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private DashboardData _data;
        public DashboardData Data { get => _data; set => Set(ref _data, value); }

        public ICommand LoadCommand { get; }
        public Action<string> NavigateCommand { get; set; }

        public DashboardViewModel()
        {
            LoadCommand = new RelayCommand(async _ => await LoadDashboard());
        }

        public async Task LoadDashboard()
        {
            try
            {
                Data = await ApiService.GetAsync<DashboardData>("dashboard/summary");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки дашборда: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}