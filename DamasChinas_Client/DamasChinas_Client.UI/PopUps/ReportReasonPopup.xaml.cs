using System;
using System.Windows;
using System.Windows.Controls;

namespace DamasChinas_Client.UI.Popups
{
    public partial class ReportReasonPopup : UserControl
    {
        public bool IsConfirmed { get; private set; }
        public string SelectedReasonKey { get; private set; }

        public event Action RequestClose;

        public ReportReasonPopup()
        {
            InitializeComponent();
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            string key = GetSelectedReasonKey();

            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            SelectedReasonKey = key;
            IsConfirmed = true;
            RequestClose?.Invoke();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            SelectedReasonKey = null;
            RequestClose?.Invoke();
        }

        private string GetSelectedReasonKey()
        {
            if (rbSpam.IsChecked == true)
            {
                return "reportReason_SpamChat";
            }

            if (rbOffensive.IsChecked == true)
            {
                return "reportReason_OffensiveLanguage";
            }

            if (rbCheat.IsChecked == true)
            {
                return "reportReason_Cheating";
            }

            if (rbBadBehavior.IsChecked == true)
            {
                return "reportReason_InappropriateBehavior";
            }

            return null;
        }
    }
}
