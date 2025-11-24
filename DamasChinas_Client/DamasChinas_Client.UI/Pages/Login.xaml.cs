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

        private void OnLoginClick(object sender, RoutedEventArgs e)
        {
            LoadingWindow loading = null;

            try
            {
                var (username, password) = GetCredentials();
                if (!ValidateCredentials(username, password))
                    return;

                string hashed = Hasher.HashPassword(password);
                loading = ShowLoading();

                var client = CreateLoginClient(out var callback);
                ConfigureCallback(callback, loading, client);
                ExecuteLogin(client, username, hashed);
            }
            catch (Exception ex) when (
                ex is EndpointNotFoundException ||
                ex is TimeoutException ||
                ex is CommunicationException)
            {
                Debug.WriteLine($"[Login.OnLoginClick] {ex}");
                loading?.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Login.OnLoginClick - General] {ex}");
                loading?.Close();
            }
        }

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

        private static LoginServiceClient CreateLoginClient(out LoginCallbackHandler callback)
        {
            callback = new LoginCallbackHandler();
            return new LoginServiceClient(new InstanceContext(callback));
        }

        private void ConfigureCallback(LoginCallbackHandler callback, LoadingWindow loading, LoginServiceClient client)
        {
            var channel = client.InnerChannel;

            channel.Faulted += (_, __) => HandleConnectionLoss(loading);
            channel.Closed += (_, __) => HandleConnectionLoss(loading);

            callback.LoginSuccess += async profile =>
            {
                await SafeWait(loading);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    TryNavigateToMenu(profile, loading);
                });
            };

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

                ClientSession.Initialize(profile);
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
