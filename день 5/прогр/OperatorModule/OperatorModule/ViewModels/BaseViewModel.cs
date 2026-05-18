using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OperatorModule.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string propName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propName);
                return true;
            }
            return false;
        }
    }
}