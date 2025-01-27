using System.Reflection;
using System.Windows;

namespace pxdArchiverCE
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        string Copyright = "(C) 2024,2025 Ret-HZ";

        public AboutWindow()
        {
            InitializeComponent();
            this.Owner = App.Current.MainWindow;
            lbl_copyright.Content = Copyright;
            lbl_Info.Content = $"PXD Archiver CE {Assembly.GetExecutingAssembly().GetName().Version.ToString()} ({(Environment.Is64BitProcess ? "64 bit" : "32 bit")})";
        }


        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
