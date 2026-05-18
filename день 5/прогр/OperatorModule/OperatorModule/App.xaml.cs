using OperatorModule.Models;
using System.Configuration;
using System.Data;
using System.Windows;

namespace OperatorModule
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static User CurrentUser { get; set; }
        public static string AuthToken { get; set; }
    }

}
