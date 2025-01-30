using System.IO;
using System.Windows;

namespace pxdArchiverCE
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            if (e.Args.Length > 0)
            {
                string filePath = e.Args[0];
                if (File.Exists(filePath))
                {
                    mainWindow.OpenPAR(filePath);
                }
            }
        }
    }

}
