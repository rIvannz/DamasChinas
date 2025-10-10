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


namespace DamasChinas_Client.UI.Pages
{
    public partial class Login : Page
    {
        public Login()
        {
            InitializeComponent();
        }

        // ===== EVENTOS =====

        /// <summary>
        /// Maneja el clic en el botón "Log in".
        /// </summary>
        private void OnLoginClick(object sender, RoutedEventArgs e)
        {
            // Navegar directamente al menú registrado sin validar credenciales.
            NavigationService?.Navigate(new MenuRegisteredPlayer());
        }

        /// <summary>
        /// Maneja el clic en el botón "Back" para regresar a la ventana principal.
        /// </summary>
        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
            else
                MessageBox.Show("No hay una página anterior para regresar.");
        }

        /// <summary>
        /// Maneja el clic en el ícono "Language" para cambiar el idioma.
        /// </summary>
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

        /// <summary>
        /// Maneja el clic en el enlace "¿Olvidaste tu contraseña?".
        /// </summary>
        private void OnForgotPasswordClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Aquí se mostrará la recuperación de contraseña.");
        }

        /// <summary>
        /// Maneja el clic en el ícono de sonido.
        /// </summary>
        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ConfiSound());
        }
    }
}


