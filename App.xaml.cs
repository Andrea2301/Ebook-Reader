using System.Configuration;
using System.Data;
using System.Windows;

namespace Ebook_Reader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = new MainWindow();
            mainWindow.DataContext = new ViewModels.MainViewModel();
            mainWindow.Show();
        }
    }

}
