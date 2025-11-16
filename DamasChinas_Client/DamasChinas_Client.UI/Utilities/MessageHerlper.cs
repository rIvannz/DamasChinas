using DamasChinas_Client.UI.PopUps;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Windows;



namespace DamasChinas_Client.UI.Utilities
{
    public static class MessageHelper
    {
   
        public static void ShowPopup(string message, string type = "info", bool autoClose = false)
        {
            var popup = new MessagePopupWindow(message, type, autoClose)
            {
                Owner = Application.Current.MainWindow
            };

            popup.ShowDialog();
        }

        public static void ShowFromCode(Enum code, string type = "info")
        {
            string message = MessageTranslator.GetLocalizedMessage(code);
            ShowPopup(message, type);
        }

        public static bool ShowConfirmLogout()
        {
            var popup = new ConfirmPopupWindow
            {
                Owner = Application.Current.MainWindow
            };

            popup.ShowDialog();
            return popup.Result;
        }



        public static void ShowFromResult(dynamic result)
        {
            if (result == null)
            {
                ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
                return;
            }

            string msg = MessageTranslator.GetLocalizedMessage(result.Code);

            if (result.Success)
            {
                ShowPopup(msg, "success");
            }
            else
            {
                ShowPopup(msg, "error");
            }
        }
    }
}


