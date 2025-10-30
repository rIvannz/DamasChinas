using DamasChinas_Client.UI.LogInServiceProxy;
using DamasChinas_Client.UI.Utilities;
using System;
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
            try
            {
                var (username, password) = GetCredentials();

                if (!ValidateCredentials(username, password))
                {
                    return;
                }

                // 🔹 Hashear la contraseña antes de enviarla al servidor
                string hashedPassword = Hasher.HashPassword(password);

                var client = CreateLoginClient(out var callback);
                ConfigureCallback(callback);

                ExecuteLogin(client, username, hashedPassword);
            }
            catch (Exception ex)
            {
                ShowError($"Error al conectar con el servicio:\n{ex.Message}");
            }
        }

        private (string username, string password) GetCredentials()
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password.Trim();
            return (username, password);
        }

        private bool ValidateCredentials(string username, string password)
        {
            try
            {
                // Puedes reactivar las validaciones si lo deseas
                // Validator.ValidateUsername(username);
                // Validator.ValidatePassword(password);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        private LogInServiceProxy.LoginServiceClient CreateLoginClient(out LoginCallbackHandler callback)
        {
            callback = new LoginCallbackHandler();
            var context = new InstanceContext(callback);
            return new LogInServiceProxy.LoginServiceClient(context);
        }

        private void ConfigureCallback(LoginCallbackHandler callback)
        {
            callback.LoginSuccess += profile =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
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
                });
            };

            callback.LoginError += message =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(message, "Login", MessageBoxButton.OK, MessageBoxImage.Warning);
                });
            };
        }

        private void ExecuteLogin(LogInServiceProxy.LoginServiceClient client, string username, string hashedPassword)
        {
            var loginRequest = new LogInServiceProxy.LoginRequest
            {
                Username = username,
                Password = hashedPassword // 🔒 Enviar hash, no texto plano
            };

            client.Login(loginRequest);
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
            else
            {
                MessageBox.Show("No hay una página anterior para regresar.");
            }
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new SelectLanguage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir la configuración de idioma: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnForgotPasswordClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Aquí se mostrará la recuperación de contraseña.");
        }

        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ConfiSound());
        }
    }
}
