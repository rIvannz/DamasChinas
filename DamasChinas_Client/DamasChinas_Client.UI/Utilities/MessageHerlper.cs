using DamasChinas_Client.UI.PopUps;
using DamasChinas_Client.UI.Pages;
using System;
using System.Collections.Generic;
using System.Windows;

namespace DamasChinas_Client.UI.Utilities
{
    public static class MessageHelper
    {
        private const PopupType DefaultType = PopupType.Info;

        private static readonly Dictionary<string, Action> SpecialKeyActions =
    new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase)
    {
        { "msg_SessionExpired", () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ClientSession.Clear();
                    Application.Current.MainWindow.Content = new MainWindow();
                });
            }
        },

        { "msg_ServerUnavailable", () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ClientSession.Clear();
                    Application.Current.MainWindow.Content = new MainWindow();
                });
            }
        },
    };


        public static void ShowPopup(string messageKey, PopupType type = DefaultType, bool autoClose = false)
        {
            string message = MessageTranslator.GetLocalizedMessage(messageKey);

            if (SpecialKeyActions.TryGetValue(messageKey, out var action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[MessageHelper] SpecialKeyAction failed: " + ex.Message);
                }
            }

            var popup = new MessagePopupWindow(message, type.ToString().ToLower(), autoClose)
            {
                Owner = Application.Current.MainWindow
            };

            if (popup.IsDuplicate)
            {
                return;
            }

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


        public static bool ShowConfirm(string messageResourceKey)
        {
            var popup = new ConfirmPopupWindow(messageResourceKey)
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

