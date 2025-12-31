using DamasChinas_Client.UI.Callbacks;
using DamasChinas_Client.UI.LogInServiceProxy;
using DamasChinas_Client.UI.PopUps;
using DamasChinas_Client.UI.SessionServiceProxy;
using DamasChinas_Client.UI.Utilities;
using DamasChinas_Shared.Contracts.Dtos;
using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static DamasChinas_Client.UI.Utilities.MessageKeys;

namespace DamasChinas_Client.UI.Pages
{
    public partial class Login : Page
    {
        public Login()
        {
            InitializeComponent();
        }

        private async void OnLoginClick(object sender, RoutedEventArgs e)
        {
            LoadingWindow loading = null;

            try
            {
                var (username, password) = GetCredentials();

                if (!ValidateCredentials(username, password))
                {
                    return;
                }

                string hashedPassword = Hasher.HashPassword(password);

                loading = ShowLoading();

                var client = CreateLoginClient(out var callback);
                ConfigureCallback(callback, loading, client);

                ExecuteLogin(client, username, hashedPassword);
            }
            catch (Exception ex) when (
                ex is EndpointNotFoundException ||
                ex is TimeoutException ||
                ex is CommunicationException)
            {
                Debug.WriteLine($"[Login.OnLoginClick - Network] {ex}");
                await SafeWait(loading);

                _ = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    loading?.Close();
                    MessageHelper.ShowPopup(ServerUnavailable, PopupType.Error);
                }));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Login.OnLoginClick - General] {ex}");
                await SafeWait(loading);

                _ = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    loading?.Close();
                    MessageHelper.ShowPopup(UnknownError, PopupType.Error);
                }));
            }
        }

        private (string username, string password) GetCredentials()
        {
            return (txtUsername.Text.Trim(), txtPassword.Password.Trim());
        }

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
            var window = new LoadingWindow
            {
                Owner = Application.Current.MainWindow
            };

            window.Show();
            return window;
        }

        private static LoginServiceClient CreateLoginClient(out LoginCallbackHandler callback)
        {
            callback = new LoginCallbackHandler();
            var context = new InstanceContext(callback);

            var client = new LoginServiceClient(context);
            callback.AttachClient(client);

            return client;
        }

        private void ConfigureCallback(LoginCallbackHandler callback, LoadingWindow loading, LoginServiceClient client)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            if (client == null) throw new ArgumentNullException(nameof(client));

            var channel = client.InnerChannel;

            channel.Faulted += (_, __) =>
            {
                if (ClientSession.IsIntentionalDisconnect)
                {
                    return;
                }

                _ = HandleConnectionLossAsync(loading);
            };

            channel.Closed += (_, __) =>
            {
                if (ClientSession.IsIntentionalDisconnect)
                {
                    return;
                }

                _ = HandleConnectionLossAsync(loading);
            };

            // =========================
            // LOGIN OK
            // =========================
            callback.LoginSuccess += async profile =>
            {
                await SafeWait(loading);

                _ = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    loading?.Close();

                    ClientSession.Initialize(profile, client, callback);

                    try
                    {
                        var sessionCallback = new SessionCallbackHandler();
                        var ctx = new InstanceContext(sessionCallback);
                        var sessionClient = new SessionServiceClient(ctx);

                        sessionClient.Subscribe(profile.Username);
                        ClientSession.SessionClient = sessionClient;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[Login.ConfigureCallback] Session subscribe error: {ex.Message}");
                    }

                    TryNavigateToMenu(profile);
                }));
            };

            // =========================
            // LOGIN ERROR
            // =========================
            callback.LoginError += async code =>
            {
                await SafeWait(loading);

                _ = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    loading?.Close();
                    MessageHelper.ShowFromCode(code, PopupType.Warning);
                }));
            };

            // =========================
            // LOGIN BANNED
            // =========================
            callback.LoginBanned += async banInfo =>
            {
                await SafeWait(loading);

                _ = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    loading?.Close();

                    PendingBanNotificationStore.Save(banInfo);

                    string msg = PendingBanNotificationStore.BuildBanMessage(banInfo);
                    MessageHelper.ShowPopup(msg, PopupType.Error);

                    PendingBanNotificationStore.Clear();

                    try
                    {
                        // Corte de raíz: evita Closed/Faulted “en cascada”
                        ClientSession.ClearForced();
                    }
                    catch
                    {
                    }

                    AppNavigator.NavigateToRoot(new MainWindow());
                }));
            };
        }

        private static async Task HandleConnectionLossAsync(LoadingWindow loading)
        {
            await SafeWait(loading);

            _ = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                // Si ya fue intencional (logout/ban), no muestres error
                if (ClientSession.IsIntentionalDisconnect)
                {
                    loading?.Close();
                    return;
                }

                loading?.Close();
                MessageHelper.ShowPopup(ServerUnavailable, PopupType.Error);
            }));
        }

        private static async Task SafeWait(LoadingWindow loading)
        {
            if (loading == null)
            {
                return;
            }

            try
            {
                await loading.WaitMinimumAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Login.SafeWait] {ex}");
            }
        }

        private void TryNavigateToMenu(PublicProfile profile)
        {
            try
            {
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

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NavigationService?.CanGoBack == true)
                {
                    NavigationService.GoBack();
                }
                else
                {
                    MessageHelper.ShowPopup(NavigationError, PopupType.Warning);
                }
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
                action?.Invoke();
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
