using System;
using System.Windows;
using System.Windows.Controls;
using DamasChinas_Client.UI.Pages;
using DamasChinas_Client.UI.AccountManagerServiceProxy;

namespace DamasChinas_Client.UI.Pages
{
    public partial class ChangeData : Page
    {
        private int _idUsuario;

        public ChangeData()
        {
            InitializeComponent();
        }

        // Constructor con idUsuario para cargar los datos
        public ChangeData(int idUsuario) : this()
        {
            _idUsuario = idUsuario;

            try
            {
                var client = new AccountManagerClient();
                var profile = client.ObtenerPerfilPublico(_idUsuario);

                if (profile != null)
                {
                    txtFirstName.Text = profile.Nombre;
                    txtLastName.Text = profile.LastName;
                    txtEmail.Text = profile.Correo;
                    txtCurrentUsername.Text = profile.Username;
                }
                else
                {
                    MessageBox.Show("No se encontró el perfil del usuario.",
                                    "Perfil", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los datos del usuario: " + ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== BOTONES =====

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ProfilePlayer(null, _idUsuario));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnSendCodeClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Código enviado correctamente.",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ConfiSound());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"Error inesperado:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== CAMBIO DE USERNAME =====
        private void OnSaveUsernameClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    MessageBox.Show("El nombre de usuario no puede estar vacío.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var client = new AccountManagerClient();
                var resultado = client.CambiarUsername(_idUsuario, txtUsername.Text);

                if (resultado.Exito)
                {
                    MessageBox.Show(resultado.Mensaje,
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Volver al perfil actualizado
                    NavigationService?.Navigate(new ProfilePlayer(client.ObtenerPerfilPublico(_idUsuario), _idUsuario));
                }
                else
                {
                    MessageBox.Show(resultado.Mensaje,
                        "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cambiar el nombre de usuario: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== CAMBIO DE CONTRASEÑA =====
        private void OnSavePasswordClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtPassword.Password) ||
                    string.IsNullOrWhiteSpace(txtConfirmPassword.Password))
                {
                    MessageBox.Show("Por favor llena todos los campos.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (txtPassword.Password != txtConfirmPassword.Password)
                {
                    MessageBox.Show("Las contraseñas no coinciden.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var client = new AccountManagerClient();
                var resultado = client.CambiarPassword(_idUsuario, txtPassword.Password);

                if (resultado.Exito)
                {
                    MessageBox.Show(resultado.Mensaje,
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    txtPassword.Password = string.Empty;
                    txtConfirmPassword.Password = string.Empty;
                }
                else
                {
                    MessageBox.Show(resultado.Mensaje,
                        "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cambiar la contraseña: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
