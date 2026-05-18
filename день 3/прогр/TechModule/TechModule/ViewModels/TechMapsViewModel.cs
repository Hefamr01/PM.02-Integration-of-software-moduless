using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TechModule.Models;
using TechModule.Services;

namespace TechModule.ViewModels
{
    public class TechMapsViewModel : BaseViewModel
    {
        public ObservableCollection<TechMap> TechMaps { get; } = new ObservableCollection<TechMap>();
        private TechMap _selectedTechMap;
        public TechMap SelectedTechMap
        {
            get => _selectedTechMap;
            set
            {
                Set(ref _selectedTechMap, value);
                ((RelayCommand)EditCommand)?.CanExecute(null);
                ((RelayCommand)ActivateCommand)?.CanExecute(null);
                ((RelayCommand)ArchiveCommand)?.CanExecute(null);
                ((RelayCommand)DeleteCommand)?.CanExecute(null);
            }
        }

        public ICommand LoadCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ActivateCommand { get; }
        public ICommand ArchiveCommand { get; }

        public Action<TechMap> OnEditTechMap { get; set; }

        public TechMapsViewModel()
        {
            LoadCommand = new RelayCommand(async _ => await LoadTechMaps());
            AddCommand = new RelayCommand(_ => OnEditTechMap?.Invoke(null));
            EditCommand = new RelayCommand(_ => OnEditTechMap?.Invoke(SelectedTechMap), _ => SelectedTechMap != null);
            DeleteCommand = new RelayCommand(async _ => await DeleteTechMap(), _ => SelectedTechMap != null);
            ActivateCommand = new RelayCommand(async _ => await ActivateTechMap(), _ => SelectedTechMap != null && SelectedTechMap.status != "active");
            ArchiveCommand = new RelayCommand(async _ => await ArchiveTechMap(), _ => SelectedTechMap != null && SelectedTechMap.status != "archived");
        }

        public async Task LoadTechMaps()
        {
            try
            {
                var list = await ApiService.GetAsync<System.Collections.Generic.List<TechMap>>("tech_maps");
                TechMaps.Clear();
                foreach (var tm in list) TechMaps.Add(tm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки техкарт: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteTechMap()
        {
            if (SelectedTechMap == null) return;
            try
            {
                await ApiService.DeleteAsync($"tech_maps/{SelectedTechMap.id}");
                TechMaps.Remove(SelectedTechMap);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ActivateTechMap()
        {
            try
            {
                await ApiService.ActivateTechMapAsync<TechMap>(SelectedTechMap.id);
                await LoadTechMaps();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка активации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ArchiveTechMap()
        {
            try
            {
                await ApiService.ArchiveTechMapAsync<TechMap>(SelectedTechMap.id);
                await LoadTechMaps();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка архивации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}