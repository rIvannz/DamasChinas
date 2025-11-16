using DamasChinas_Client.UI.LogInServiceProxy;
using DamasChinas_Client.UI.PopUps;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;


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
            LoadingWindow loadingWindow = null;

            try
            {
                var (username, password) = GetCredentials();

                if (!ValidateCredentials(username, password))
                {
                    return;
                }

                string hashedPassword = Hasher.HashPassword(password);

                // ============================
                // Mostrar ventana de carga
                // ============================
                loadingWindow = new LoadingWindow
                {
                    Owner = Application.Current.MainWindow
                };
                loadingWindow.Show();

                var client = CreateLoginClient(out var callback);
                ConfigureCallback(callback, loadingWindow);

                ExecuteLogin(client, username, hashedPassword);
            }
            catch (EndpointNotFoundException ex)
            {
                Debug.WriteLine($"[Login.OnLoginClick - EndpointNotFound] {ex.Message}");
                loadingWindow?.Close();

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_ServerUnavailable"),
                    "error"
                );
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"[Login.OnLoginClick - Timeout] {ex.Message}");
                loadingWindow?.Close();

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NetworkLatency"),
                    "error"
                );
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[Login.OnLoginClick - Communication] {ex.Message}");
                loadingWindow?.Close();

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_ServerUnavailable"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Login.OnLoginClick - General] {ex.Message}");
                loadingWindow?.Close();

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }



        // ============================================================
        // 🔹 CREDENTIALS RETRIEVAL
        // ============================================================

        private (string username, string password) GetCredentials()
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password.Trim();
            return (username, password);
        }

        // ============================================================
        // 🔹 VALIDATION
        // ============================================================

        private bool ValidateCredentials(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_EmptyCredentials"),
                    "warning"
                );
                return false;
            }

            // If you want to enable internal validation:
            /*
			try
			{
				Validator.ValidateUsername(username);
				Validator.ValidatePassword(password);
			}
			catch (ArgumentException ex)
			{
				MessageHelper.ShowPopup(
					MessageTranslator.GetLocalizedMessage("msg_InvalidCredentials"),
					"warning"
				);
				return false;
			}
			*/

            return true;
        }

    

        private LogInServiceProxy.LoginServiceClient CreateLoginClient(out LoginCallbackHandler callback)
        {
            callback = new LoginCallbackHandler();
            var context = new InstanceContext(callback);
            return new LogInServiceProxy.LoginServiceClient(context);
        }



        private void ConfigureCallback(LoginCallbackHandler callback, LoadingWindow loadingWindow)
        {
       
            callback.LoginSuccess += async profile =>
            {
                try
                {
                    // Esperar tiempo mínimo de loading
                    await loadingWindow.WaitMinimumAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Login.CallbackSuccess - WaitMinimum] {ex.Message}");
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        if (loadingWindow.IsVisible)
                        {
                            loadingWindow.Close();
                        }

                        var convertedProfile = new AccountManagerServiceProxy.PublicProfile
                        {
                            Name = profile.Name,
                            Username = profile.Username,
                            Email = profile.Email,
                            LastName = profile.LastName,
                            SocialUrl = profile.SocialUrl,
                        };

                        ClientSession.Initialize(profile);

                        var menuPage = new MenuRegisteredPlayer(convertedProfile);
                        NavigationService?.Navigate(menuPage);
                    }
                    catch (InvalidOperationException ex)
                    {
                        Debug.WriteLine($"[Login.CallbackSuccess - InvalidOperation] {ex.Message}");

                        MessageHelper.ShowPopup(
                            MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                            "error"
                        );
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[Login.CallbackSuccess - General] {ex.Message}");

                        MessageHelper.ShowPopup(
                            MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                            "error"
                        );
                    }
                });
            };

          
            callback.LoginError += async code =>
            {
                string msg = MessageTranslator.GetLocalizedMessage(code);

                try
                {
                    // También respetar tiempo mínimo en caso de error
                    await loadingWindow.WaitMinimumAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Login.CallbackError - WaitMinimum] {ex.Message}");
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (loadingWindow.IsVisible)
                    {
                        loadingWindow.Close();
                    }

                    MessageHelper.ShowPopup(
                        msg,
                        "warning"
                    );
                });
            };
        }






        private void ExecuteLogin(LogInServiceProxy.LoginServiceClient client, string username, string hashedPassword)
        {
            try
            {
                var loginRequest = new LogInServiceProxy.LoginRequest
                {
                    Username = username,
                    Password = hashedPassword
                };

                client.Login(loginRequest);
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[Login.ExecuteLogin - Communication] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_ServerUnavailable"),
                    "error"
                );
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"[Login.ExecuteLogin - Timeout] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NetworkLatency"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Login.ExecuteLogin - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }


        private void ShowError(string message)
        {
            MessageHelper.ShowPopup(
                MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                "error"
            );
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
                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                        "warning"
                    );
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[Login.OnBackClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Login.OnBackClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new SelectLanguage());
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[Login.OnLanguageClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Login.OnLanguageClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }

        private void OnForgotPasswordClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ForgotPassword());
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[Login.OnForgotPasswordClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Login.OnForgotPasswordClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }

        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ConfiSound());
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[Login.OnSoundClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Login.OnSoundClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }
    }
}
