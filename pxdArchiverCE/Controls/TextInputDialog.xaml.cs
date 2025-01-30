using System.Windows;
using System.Windows.Input;

namespace pxdArchiverCE.Controls
{
    /// <summary>
    /// Interaction logic for TextInputDialog.xaml
    /// </summary>
    public partial class TextInputDialog : Window
    {
        /// <summary>
        /// A boolean indicating if the user has cancelled the operation.
        /// </summary>
        public bool IsCancelled { get; private set; }

        /// <summary>
        /// The user input.
        /// </summary>
        public string Input { get; private set; }

        private string originalText = string.Empty;


        public TextInputDialog(string title, string placeholderText)
        {
            this.Owner = Application.Current.MainWindow;
            InitializeComponent();
            IsCancelled = true; // True unless set otherwise
            this.Title = title;
            tb_Text.Text = placeholderText;
            tb_Text.Focus();
            tb_Text.SelectAll();
        }


        private void Save()
        {
            if (IsCancelled)
            {
                Input = originalText;
            }
            else
            {
                Input = tb_Text.Text;
            }
        }


        private void tb_Text_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                IsCancelled = false;
                this.Close();
            }
            else if (e.Key == Key.Escape)
            {
                IsCancelled = true;
                this.Close();
            }
        }


        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            IsCancelled = false;
            this.Close();
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Save();
        }
    }
}
