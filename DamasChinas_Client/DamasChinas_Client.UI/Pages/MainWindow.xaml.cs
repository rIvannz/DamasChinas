using DamasChinas_Client.UI.Pages;
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

namespace DamasChinas_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Page
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Cierra completamente la aplicación.
        /// </summary>
        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Navega a la página de inicio de sesión.
        /// </summary>
        private void OnGoToLoginClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Login());
        }

        /// <summary>
        /// Navega a la página de registro.
        /// </summary>
        private void OnGoToSignInClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SignIn());
        }

        /// <summary>
        /// Inicia el modo invitado.
        /// </summary>
        private void OnPlayAsGuestClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new MenuGuest());
        }

        /// <summary>
        /// Abre la configuración de sonido.
        /// </summary>
        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ConfiSound());
        }

        /// <summary>
        /// Maneja el clic en el ícono de idioma.
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
    }
}