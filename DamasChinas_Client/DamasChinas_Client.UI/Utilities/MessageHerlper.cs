using System.Windows;
using DamasChinas_Client.UI.Utilities;


namespace DamasChinas_Client.UI.Utilities
{
    public static class MessageHelper
    {
        public static void ShowSuccess(string message, string title = "Success")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ShowError(string message, string title = "Error")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowWarning(string message, string title = "Warning")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static void ShowInfo(string message, string title = "Information")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Displays a localized message from any OperationResult object.
        /// </summary>
        public static void ShowFromResult(dynamic result)
        {
            if (result == null)
            {
                ShowError("An unexpected error occurred.");
                return;
            }

            string message = MessageTranslator.GetLocalizedMessage(result.Code);

            if (result.Success)
                ShowSuccess(message);
            else
                ShowError(message);
        }
    }
}
