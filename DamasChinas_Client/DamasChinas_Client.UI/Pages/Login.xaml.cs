using DamasChinas_Client.UI.LogInServiceProxy;
using System;
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

        // ===== EVENTOS =====

        private void OnLoginClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var loginClient = new LogInServiceProxy.ILoginServiceClient();

                string username = txtUsername.Text.Trim();
                string password = txtPassword.Password.Trim();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Por favor ingresa usuario y contraseña.",
                                    "Login",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                var loginResult = loginClient.ValidateLogin(username, password);

                if (loginClient.State == System.ServiceModel.CommunicationState.Faulted)
                {
                    loginClient.Abort();
                }
                else
                {
                    loginClient.Close();
                }

                if (loginResult != null && loginResult.Success)
                {
                
                    NavigationService?.Navigate(new MenuRegisteredPlayer(loginResult.IdUsuario, loginResult.Username));
                }
                else
                {
                    MessageBox.Show("Login fallido. Usuario o contraseña incorrectos.",
                                    "Login",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con el servicio:\n{ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
            else
                MessageBox.Show("No hay una página anterior para regresar.");
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new SelectLanguage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir la configuración de idioma: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
