// App.xaml.cs
using System.Windows;
using TechModule.Models;

namespace TechModule
{
    public partial class App : Application
    {
        public static User CurrentUser { get; set; }
        public static string AuthToken { get; set; }
    }
}