using DamasChinas_Client.UI.PopUps;
using DamasChinas_Client.UI.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace DamasChinas_Client.UI.Utilities
{
    public static class MessageHelper
    {
        private const PopupType DefaultType = PopupType.Info;

        // OJO: aquí NO usamos Invoke (bloqueante). Todo será BeginInvoke.
        // Además: si la desconexión fue intencional, NO dispares acciones de "ServerUnavailable".
        private static readonly Dictionary<string, Action> SpecialKeyActions =
            new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase)
            {
                { MessageKeys.SessionExpired, () =>
                    {
                        // Si fue intencional (logout/ban), no hagas redirect “como error”
                        if (ClientSession.IsIntentionalDisconnect)
                            return;

                        SafeUi(() =>
                        {
                            try { ClientSession.ClearForced(); } catch { }
                            AppNavigator.NavigateToRoot(new MainWindow());
                        });
                    }
                },

                { MessageKeys.ServerUnavailable, () =>
                    {
                        if (ClientSession.IsIntentionalDisconnect)
                            return;

                        SafeUi(() =>
                        {
                            try { ClientSession.ClearForced(); } catch { }
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

            // 1) Ejecuta acciones especiales (sin bloquear UI)
            if (SpecialKeyActions.TryGetValue(messageKey, out var action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[MessageHelper] SpecialKeyAction failed: " + ex.Message);
                }
            }

            // 2) Muestra el popup SIEMPRE en UI thread y sin Invoke
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

                // Nota: ShowDialog bloquea (es modal), pero aquí ya estás en UI thread correctamente.
                // Si aun así te “congela” por llamadas consecutivas, se puede migrar a Show() + autoclose.
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
            // Confirm necesita ser modal y retornar bool, así que sí o sí debe correrse en UI thread.
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

        // ==========================
        // Helpers
        // ==========================
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
            catch (Exception ex)
            {
                Debug.WriteLine($"[MessageHelper.SafeUi] {ex.Message}");
            }
        }
    }
}
