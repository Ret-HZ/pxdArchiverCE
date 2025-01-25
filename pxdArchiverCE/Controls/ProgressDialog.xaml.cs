using System.Windows;

namespace pxdArchiverCE.Controls
{
    /// <summary>
    /// Interaction logic for ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialog : Window
    {
        /// <summary>
        /// A value that indicates if the current operation has been cancelled by the user.
        /// </summary>
        public bool IsCancelledByUser { get; private set; }


        public ProgressDialog(string windowTitle = "", string text = "", string description = "")
        {
            InitializeComponent();
            SetTitle(windowTitle);
            SetText(text);
            SetDescription(description);
        }


        /// <summary>
        /// Sets the progress window title.
        /// </summary>
        /// <param name="title">The new window title.</param>
        public void SetTitle(string title)
        {
            this.Title = title;
        }


        /// <summary>
        /// Sets the progress window header text.
        /// </summary>
        /// <param name="text">The new header text.</param>
        public void SetText(string text)
        {
            lbl_Text.Content = text;
        }


        /// <summary>
        /// Sets the progress window description text.
        /// </summary>
        /// <param name="description">The new description.</param>
        public void SetDescription(string description)
        {
            tb_Description.Text = description;
        }


        /// <summary>
        /// Sets the progress bar amount.
        /// </summary>
        /// <param name="progress">The new progress bar amount.</param>
        public void SetProgress(int progress)
        {
            pb_Progress.Value = progress;
        }


        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsCancelledByUser = true;
            btn_Cancel.IsEnabled = false;
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsCancelledByUser)
            {
                return;
            }
            else
            {
                IsCancelledByUser = true;
                btn_Cancel.IsEnabled = false;
                e.Cancel = true;
            }
        }
    }
}
