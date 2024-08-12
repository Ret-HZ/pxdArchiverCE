using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace pxdArchiverCE.Controls
{
    /// <summary>
    /// Interaction logic for TouchSetTimeDialog.xaml
    /// </summary>
    public partial class TouchSetTimeDialog : Window
    {
        /// <summary>
        /// The name of the target file.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The original, unmodified date of the target file.
        /// </summary>
        public DateTime OriginalDate { get; set; }

        /// <summary>
        /// The new date for the target file.
        /// </summary>
        public DateTime NewDate { get; set; }

        /// <summary>
        /// Indicates if the "Bulk" checkbox has been checked.
        /// </summary>
        public bool IsBulk { get; set; }

        /// <summary>
        /// Indicates if the "Ignore" button has been pressed.
        /// </summary>
        public bool IsIgnore { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="TouchSetTimeDialog"/> class.
        /// </summary>
        /// <param name="fileName">The name of the target file.</param>
        /// <param name="originalDate">The original, unmodified date of the target file.</param>
        public TouchSetTimeDialog(string fileName, DateTime originalDate)
        {
            this.Owner = Application.Current.MainWindow;
            InitializeComponent();
            this.DataContext = this;

            FileName = fileName;
            OriginalDate = originalDate;
            NewDate = originalDate;
        }


        /// <summary>
        /// TextChanged event for the "NewDate" TextBox. Will check if the string can be parsed into a valid DateTime and toggle the "touch" button accordingly.
        /// </summary>
        private void tb_NewDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                CultureInfo culture = CultureInfo.CreateSpecificCulture("en-GB");
                DateTime.Parse(tb_NewDate.Text, culture);
                btn_Touch.IsEnabled = true;
            }
            catch
            {
                btn_Touch.IsEnabled = false;
            }
        }


        /// <summary>
        /// Click event for the "touch" Button. Will set the DialogResult and close the window.
        /// </summary>
        private void btn_Touch_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }


        /// <summary>
        /// Click event for the "Ignore" Button. Will set the DialogResult and close the window.
        /// </summary>
        private void btn_Ignore_Click(object sender, RoutedEventArgs e)
        {
            this.IsIgnore = true;
            this.DialogResult = true;
            this.Close();
        }


        /// <summary>
        /// Click event for the "Abort" Button. Will set the DialogResult and close the window.
        /// </summary>
        private void btn_Abort_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
