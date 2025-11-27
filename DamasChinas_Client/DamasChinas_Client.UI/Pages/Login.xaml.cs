using DamasChinas_Client.UI.LogInServiceProxy;
using DamasChinas_Client.UI.PopUps;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using static DamasChinas_Client.UI.Utilities.MessageKeys;

namespace DamasChinas_Client.UI.Pages
{
    public partial class Login : Page
    {
        public Login()
        {
            InitializeComponent();
        }

        // ============================================================
        // LOGIN CLICK
        // ============================================================
        private void OnLoginClick(object sender, RoutedEventArgs e)
        {
           // btnLogin.IsEnabled = false;
            LoadingWindow loading = null;

            try
            {
                var (username, password) = GetCredentials();

                if (!ValidateCredentials(username, password))
                {
                  //  btnLogin.IsEnabled = true;
                    return;
                }

                var hashedPassword = Hasher.HashPassword(password);
                loading = ShowLoading();

                var client = CreateLoginClient(out var callback);
                ConfigureCallback(callback, loading, client);

                ExecuteLogin(client, username, hashedPassword);
            }
            catch (EndpointNotFoundException ex)
            {
                Debug.WriteLine($"[Login.OnLoginClick - EndpointNotFound] {ex}");
                MessageHelper.ShowPopup(ServerUnavailable, PopupType.Error);
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"[Login.OnLoginClick - Timeout] {ex}");
             //   MessageHelper.ShowPopup(NetworkTimeout, PopupType.Warning);
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[Login.OnLoginClick - Communication] {ex}");
            //    MessageHelper.ShowPopup(CommunicationFailed, PopupType.Error);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[Login.OnLoginClick - InvalidOperation] {ex}");
                MessageHelper.ShowPopup(UnknownError, PopupType.Error);
            }
            finally
            {
                loading?.Close();

                if (!ClientSession.IsLoggedIn)
                {
              //      btnLogin.IsEnabled = true;
                }
            }
        }


        // ============================================================
        // UTILIDADES INPUT
        // ============================================================
        private (string username, string password) GetCredentials()
            => (txtUsername.Text.Trim(), txtPassword.Password.Trim());

        private static bool ValidateCredentials(string u, string p)
        {
            if (string.IsNullOrWhiteSpace(u) || string.IsNullOrWhiteSpace(p))
            {
                MessageHelper.ShowPopup(EmptyCredentials, PopupType.Warning);
                return false;
            }
            return true;
        }

        private LoadingWindow ShowLoading()
        {
            var w = new LoadingWindow { Owner = Application.Current.MainWindow };
            w.Show();
            return w;
        }

        // ============================================================
        // CREACIÓN DEL CLIENTE
        // ============================================================
        private static LoginServiceClient CreateLoginClient(out LoginCallbackHandler callback)
        {
            callback = new LoginCallbackHandler();
            return new LoginServiceClient(new InstanceContext(callback));
        }

        // ============================================================
        // CALLBACKS WCF
        // ============================================================
        private void ConfigureCallback(LoginCallbackHandler callback, LoadingWindow loading, LoginServiceClient client)
        {
            var channel = client.InnerChannel;

            channel.Faulted += (_, __) => HandleConnectionLoss(loading);
            channel.Closed += (_, __) => HandleConnectionLoss(loading);

            // ---------------------------
            // LOGIN SUCCESS
            // ---------------------------
            callback.LoginSuccess += async profile =>
            {
                await SafeWait(loading);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    // ⭐⭐⭐ IMPORTANTE ⭐⭐⭐
                    // Guardamos la sesión global para que el callback NO muera
                    ClientSession.Initialize(profile, client, callback);

                    TryNavigateToMenu(profile, loading);
                });
            };

            // ---------------------------
            // LOGIN ERROR
            // ---------------------------
            callback.LoginError += async code =>
            {
                string msg = MessageTranslator.GetLocalizedMessage(code);

                await SafeWait(loading);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    loading.Close();
                    MessageHelper.ShowPopup(msg, PopupType.Warning);
                });
            };
        }

        // ============================================================
        // CONEXIÓN PERDIDA
        // ============================================================
        private static void HandleConnectionLoss(LoadingWindow loading)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (loading.IsVisible) loading.Close();
                MessageHelper.ShowPopup(ServerUnavailable, PopupType.Error);
            });
        }

        private static async System.Threading.Tasks.Task SafeWait(LoadingWindow loading)
        {
            try { await loading.WaitMinimumAsync(); }
            catch (Exception ex) { Debug.WriteLine($"[Login.SafeWait] {ex}"); }
        }

        // ============================================================
        // NAVEGAR AL MENÚ
        // ============================================================
        private void TryNavigateToMenu(PublicProfile profile, LoadingWindow loading)
        {
            try
            {
                if (loading.IsVisible) loading.Close();

                var converted = new AccountManagerServiceProxy.PublicProfile
                {
                    Name = profile.Name,
                    Username = profile.Username,
                    Email = profile.Email,
                    LastName = profile.LastName,
                    SocialUrl = profile.SocialUrl
                };

                NavigationService?.Navigate(new MenuRegisteredPlayer(converted));
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[Login.TryNavigateToMenu - InvalidOp] {ex}");
                MessageHelper.ShowPopup(NavigationError, PopupType.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Login.TryNavigateToMenu - General] {ex}");
                MessageHelper.ShowPopup(UnknownError, PopupType.Error);
            }
        }

        // ============================================================
        // EJECUTAR LOGIN EN EL SERVIDOR
        // ============================================================
        private void ExecuteLogin(LoginServiceClient client, string username, string hashedPassword)
        {
            try
            {
                client.Login(new LoginRequest
                {
                    Username = username,
                    Password = hashedPassword
                });
            }
            catch (Exception ex) when (
                ex is CommunicationException ||
                ex is TimeoutException)
            {
                Debug.WriteLine($"[Login.ExecuteLogin - Network] {ex}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Login.ExecuteLogin - General] {ex}");
            }
        }

        // ============================================================
        // BOTONES Y NAVEGACIÓN
        // ============================================================
        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NavigationService?.CanGoBack == true)
                    NavigationService.GoBack();
                else
                    MessageHelper.ShowPopup(NavigationError, PopupType.Warning);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Login.OnBackClick] {ex}");
                MessageHelper.ShowPopup(NavigationError, PopupType.Error);
            }
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            TryNavigate(() => NavigationService?.Navigate(new SelectLanguage()));
        }

        private void OnForgotPasswordClick(object sender, RoutedEventArgs e)
        {
            TryNavigate(() => NavigationService?.Navigate(new ForgotPassword()));
        }

        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            TryNavigate(() => NavigationService?.Navigate(new ConfiSound()));
        }

        private static void TryNavigate(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[Login.TryNavigate - InvalidOp] {ex}");
                MessageHelper.ShowPopup(NavigationError, PopupType.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Login.TryNavigate - General] {ex}");
                MessageHelper.ShowPopup(UnknownError, PopupType.Error);
            }
        }
    }
}
