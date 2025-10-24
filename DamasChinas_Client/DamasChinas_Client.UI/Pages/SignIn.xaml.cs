using DamasChinas_Client.UI.SingInServiceProxy;
using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
namespace DamasChinas_Client.UI.Pages
{
    public partial class SignIn : Page
    {
        private SingInServiceClient _proxy;

        public SignIn()
        {
            InitializeComponent();
        }

        private async void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            var client = new SingInServiceClient("BasicHttpBinding_ISingInService");

            try
            {
                var resultado = client.CreateUser(firstName, lastName, email, password, username);

                if (resultado.Exito)
                {
                    MessageBox.Show(resultado.Mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Limpiar formulario
                    txtFirstName.Clear();
                    txtLastName.Clear();
                    txtEmail.Clear();
                    txtUsername.Clear();
                    txtPassword.Clear();
                    txtConfirmPassword.Clear();
                }
                else
                {
                    MessageBox.Show(resultado.Mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo conectar con el servidor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
             
                if (client.State == System.ServiceModel.CommunicationState.Opened)
                    client.Close();
            }
        }



        private void OnBackClick(object sender, RoutedEventArgs e)
        { if (NavigationService?.CanGoBack == true) NavigationService.GoBack(); else MessageBox.Show("No previous page found."); }


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
