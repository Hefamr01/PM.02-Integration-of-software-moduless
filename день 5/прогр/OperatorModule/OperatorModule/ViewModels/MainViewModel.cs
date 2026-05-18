using System;
using System.Windows.Input;

namespace OperatorModule.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public Action<string> Navigate { get; set; }
        public ICommand NavigateCommand { get; }

        public MainViewModel()
        {
            NavigateCommand = new RelayCommand(param => Navigate?.Invoke(param?.ToString()));
        }
    }
}