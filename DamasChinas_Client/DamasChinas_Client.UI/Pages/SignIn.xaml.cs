using DamasChinas_Client.UI.UsuarioServiceReference;
using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

namespace DamasChinas_Client.UI.Pages
{
    public partial class SignIn : Page
    {
        private IUsuarioService _proxy;
        private UsuarioCallback _callback;

        public SignIn()
        {
            InitializeComponent();
        }

        private async void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            string first = txtFirstName.Text;
            string last = txtLastName.Text;
            string email = txtEmail.Text;
            string user = txtUsername.Text;
            string password = txtPassword.Password; // si tienes un PasswordBox

            // Crear callback
            _callback = new UsuarioCallback();

            // Crear binding y endpoint (opcional si está en config)
            var context = new InstanceContext(_callback);
            var factory = new DuplexChannelFactory<IUsuarioService>(context, "NetTcpBinding_IUsuarioService");

            _proxy = factory.CreateChannel();

            try
            {
                // Ejecutar la llamada en un hilo separado para no congelar la UI
                await System.Threading.Tasks.Task.Run(() =>
                {
                    _proxy.CrearUsuario(first, last, email, password, user);
                });

                MessageBox.Show(
                    $"Solicitud enviada para crear cuenta:\n{first} {last}\nEmail: {email}\nUsername: {user}",
                    "Create Account",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error conectando al servidor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
            else
                MessageBox.Show("No previous page found.");
        }

        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ConfiSound());
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new SelectLanguage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error while opening language settings: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
