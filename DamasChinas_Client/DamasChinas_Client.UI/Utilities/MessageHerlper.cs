using DamasChinas_Client.UI.PopUps;
using System;
using System.Windows;

namespace DamasChinas_Client.UI.Utilities
{
    public static class MessageHelper
    {
        private const PopupType DefaultType = PopupType.Info;

        public static void ShowPopup(string message, PopupType type = DefaultType, bool autoClose = false)
        {
            var popup = new MessagePopupWindow(message, type.ToString().ToLower(), autoClose)
            {
                Owner = Application.Current.MainWindow
            };

            popup.ShowDialog();
        }

        public static void ShowFromCode(Enum code, PopupType type = DefaultType)
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
                    PopupType.Error
                );
                return;
            }

            string msg = MessageTranslator.GetLocalizedMessage(result.Code);

            ShowPopup(msg,
                result.Success ? PopupType.Success : PopupType.Error
            );
        }
    }
}
