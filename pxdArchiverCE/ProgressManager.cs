using pxdArchiverCE.Controls;

namespace pxdArchiverCE
{
    internal class ProgressManager
    {
        public int MaximumProgress {  get; private set; }

        private int LastReportedProgress = 0;

        private string WindowTitle = string.Empty;

        private string WindowText = string.Empty;

        private string WindowDescription = string.Empty;

        private ProgressDialog ProgressDialog { get; set; }


        public ProgressManager(int maximumProgress)
        {
            MaximumProgress = maximumProgress;
        }


        public void PrepareWindowText(string title, string text, string description = "")
        {
            WindowTitle = title;
            WindowText = text;
            WindowDescription = description;
        }


        public void SetDescriptionText(string description)
        {
            ProgressDialog.SetDescription(description);
        }


        public void UpdateProgress(int currentProgress)
        {
            if (ProgressDialog != null && MaximumProgress != 0)
            {
                double percentage = (double)currentProgress / MaximumProgress * 100;
                ProgressDialog.SetProgress((int)percentage);

                if (currentProgress == MaximumProgress)
                {
                    ProgressDialog.Close();
                }
            }
        }


        public void UpdateProgress()
        {
            if (ProgressDialog != null && MaximumProgress != 0)
            {
                LastReportedProgress += 1;
                double percentage = (double)LastReportedProgress / MaximumProgress * 100;
                ProgressDialog.SetProgress((int)percentage);

                if (LastReportedProgress == MaximumProgress)
                {
                    ProgressDialog.AllowClosing = true;
                    ProgressDialog.Close();
                }
            }
        }


        public void ShowProgressDialog()
        {
            ProgressDialog = new ProgressDialog(WindowTitle, WindowText, WindowDescription);
            if (MaximumProgress > 5) ProgressDialog.ShowDialog();
        }


        public void ShowProgressDialogNonBlocking()
        {
            ProgressDialog = new ProgressDialog(WindowTitle, WindowText, WindowDescription);
            if (MaximumProgress > 5) ProgressDialog.Show();
        }


        public void CloseProgressDialog()
        {
            ProgressDialog.Close();
        }


        public bool CheckIsCancelledByUser()
        {
            if (ProgressDialog != null)
            {
                return ProgressDialog.IsCancelledByUser;
            }
            else return false;
        }
    }
}
