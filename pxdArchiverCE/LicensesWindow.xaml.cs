using System.IO;
using System.Windows;

namespace pxdArchiverCE
{
    /// <summary>
    /// Interaction logic for LicensesWindow.xaml
    /// </summary>
    public partial class LicensesWindow : Window
    {
        public LicensesWindow()
        {
            InitializeComponent();
            this.Owner = App.Current.MainWindow;
            tb_LicenseText.Text = GetLicenseText();
        }


        private string GetLicenseText()
        {
            using (StreamReader reader = new StreamReader(Util.GetEmbeddedFile("Licenses.txt")))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
