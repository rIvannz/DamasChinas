using DamasChinas_Client.UI.Pages;
using DamasChinas_Client.UI.PopUps;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DamasChinas_Client.UI.Utilities
{
    public static class MessageHelper
    {
        private const PopupType DefaultType = PopupType.Info;


        private static readonly Dictionary<string, Action> SpecialKeyActions =
            new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase)
            {
                { MessageKeys.SessionExpired, () =>
                    {
                        if (ClientSession.IsIntentionalDisconnect)
                        {
                            return;
                        }
                        SafeUi(() =>
                        {
                            try 
                            {
                                ClientSession.ClearForced(); 
                            }
                            catch 
                            {
                             ShowPopup(MessageKeys.UserNotFound, PopupType.Error);
                            }
                            AppNavigator.NavigateToRoot(new MainWindow());
                        });
                    }
                },

                { MessageKeys.ServerUnavailable, () =>
                    {
                        if (ClientSession.IsIntentionalDisconnect)
                        {
                            return;
                        }
                        SafeUi(() =>
                        {
                            try 
                            {
                                ClientSession.ClearForced(); 
                            }
                            catch
                            {
                             ShowPopup(MessageKeys.UserNotFound, PopupType.Error);
                            }
                            AppNavigator.NavigateToRoot(new MainWindow());
                        });
                    }
                },

                { MessageKeys.DatabaseUnavailable, () =>
                    {
                        if (ClientSession.IsIntentionalDisconnect)
                            return;

                        SafeUi(() =>
                        {
                            try 
                            {
                                ClientSession.ClearForced(); 
                            }
                            catch 
                            {
                             ShowPopup(MessageKeys.UserNotFound, PopupType.Error);
                            }
                            AppNavigator.NavigateToRoot(new MainWindow());
                        });
                    }
                },
            };

        public static void ShowPopup(string messageKey, PopupType type = DefaultType, bool autoClose = false)
        {
            if (string.IsNullOrWhiteSpace(messageKey))
            {
                messageKey = MessageKeys.UnknownError;
            }

          
            if (SpecialKeyActions.TryGetValue(messageKey, out var action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (TargetInvocationException ex)
                {
                    Debug.WriteLine("[MessageHelper] SpecialKeyAction failed: " + ex.InnerException?.Message);
                }
                catch (InvalidOperationException ex)
                {
                    Debug.WriteLine("[MessageHelper] SpecialKeyAction failed: " + ex.Message);
                }
            }

          
            SafeUi(() =>
            {
                string message = MessageTranslator.GetLocalizedMessage(messageKey);

                var popup = new MessagePopupWindow(message, type.ToString().ToLower(), autoClose)
                {
                    Owner = Application.Current?.MainWindow
                };

                if (popup.IsDuplicate)
                {
                    return;
                }

                popup.ShowDialog();
            });
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
            if (Application.Current?.Dispatcher == null)
            {
                return false;
            }

            bool result = false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var popup = new ConfirmPopupWindow(messageResourceKey)
                {
                    Owner = Application.Current.MainWindow
                };

                popup.ShowDialog();
                result = popup.Result;
            });

            return result;
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

    
        private static void SafeUi(Action uiAction)
        {
            try
            {
                var dispatcher = Application.Current?.Dispatcher;

                if (dispatcher == null)
                {
                    return;
                }

                if (dispatcher.CheckAccess())
                {
                    uiAction?.Invoke();
                    return;
                }

                dispatcher.BeginInvoke(uiAction, DispatcherPriority.Normal);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[MessageHelper.SafeUi] Invalid operation: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"[MessageHelper.SafeUi] Dispatcher canceled: {ex.Message}");
            }
            catch (TargetInvocationException ex)
            {
                Debug.WriteLine($"[MessageHelper.SafeUi] UI action failed: {ex.InnerException?.Message}");
            }
        }
    }
}
