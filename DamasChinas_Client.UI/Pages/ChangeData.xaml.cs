using DamasChinas_Client.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Runtime.CompilerServices.RuntimeHelpers;


namespace DamasChinas_Client.UI.Pages
{
    /// <summary>
    /// Interaction logic for ChangeData.xaml
    /// </summary>
    public partial class ChangeData : Page
    {
        public ChangeData()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Regresa a la ventana del perfil del jugador.
        /// </summary>
        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ProfilePlayer());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{TryFindResource("unexpectedError")}\n{ex.Message}",
                    TryFindResource("errorTitle")?.ToString() ?? "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Envía un código de verificación al correo electrónico del usuario.
        /// </summary>
        private void OnSendCodeClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Simulación de envío del código
                MessageBox.Show(TryFindResource("codeSentMessage")?.ToString() ?? "Code sent successfully.",
                    TryFindResource("success")?.ToString() ?? "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{TryFindResource("unexpectedError")}\n{ex.Message}",
                    TryFindResource("errorTitle")?.ToString() ?? "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Guarda el nuevo nombre de usuario ingresado.
        /// </summary>
        private void OnSaveUsernameClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    MessageBox.Show(TryFindResource("emptyCredentials")?.ToString(),
                        TryFindResource("errorTitle")?.ToString() ?? "Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show(TryFindResource("usernameUpdated")?.ToString() ?? "Username updated successfully.",
                    TryFindResource("success")?.ToString() ?? "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{TryFindResource("unexpectedError")}\n{ex.Message}",
                    TryFindResource("errorTitle")?.ToString() ?? "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Guarda la nueva contraseña ingresada (previa verificación del código).
        /// </summary>
        private void OnSavePasswordClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtCode.Text) ||
                    string.IsNullOrWhiteSpace(txtPassword.Password) ||
                    string.IsNullOrWhiteSpace(txtConfirmPassword.Password))
                {
                    MessageBox.Show(TryFindResource("emptyCredentials")?.ToString(),
                        TryFindResource("errorTitle")?.ToString() ?? "Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (txtPassword.Password != txtConfirmPassword.Password)
                {
                    MessageBox.Show(TryFindResource("passwordDontMatchErrorMessage")?.ToString(),
                        TryFindResource("errorTitle")?.ToString() ?? "Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show(TryFindResource("passwordUpdated")?.ToString() ?? "Password updated successfully.",
                    TryFindResource("success")?.ToString() ?? "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{TryFindResource("unexpectedError")}\n{ex.Message}",
                    TryFindResource("errorTitle")?.ToString() ?? "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Navega a la ventana de configuración de sonido.
        /// </summary>
        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ConfiSound());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{TryFindResource("unexpectedError")}\n{ex.Message}",
                    TryFindResource("errorTitle")?.ToString() ?? "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Navega a la ventana de selección de idioma.
        /// </summary>
        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new SelectLanguage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{TryFindResource("unexpectedError")}\n{ex.Message}",
                    TryFindResource("errorTitle")?.ToString() ?? "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}


