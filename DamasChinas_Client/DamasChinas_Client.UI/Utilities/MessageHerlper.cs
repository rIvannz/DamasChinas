using DamasChinas_Client.UI.PopUps;
using System;
using System.Windows;

namespace DamasChinas_Client.UI.Utilities
{
    public static class MessageHelper
    {
        private const PopupType DefaultType = PopupType.Info;

    
        public static void ShowPopup(string messageKey, PopupType type = DefaultType, bool autoClose = false)
        {
            string message = MessageTranslator.GetLocalizedMessage(messageKey);

            var popup = new MessagePopupWindow(message, type.ToString().ToLower(), autoClose)
            {
                Owner = Application.Current.MainWindow
            };

            popup.ShowDialog();
        }

        public static void ShowFromCode(Enum code, PopupType type = DefaultType)
        {
            if (code == null)
            {
                ShowPopup(MessageKeys.UnknownError, PopupType.Error);
                return;
            }

       
            string resourceKey = "msg_" + code.ToString();

            ShowPopup(resourceKey, type);
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
                ShowPopup(MessageKeys.UnknownError, PopupType.Error);
                return;
            }

            var popupType = result.Success ? PopupType.Success : PopupType.Error;

            string key = result.Code?.ToString() ?? nameof(MessageKeys.UnknownError);
            string resourceKey = "msg_" + key;

            ShowPopup(resourceKey, popupType);
        }
    }
}

