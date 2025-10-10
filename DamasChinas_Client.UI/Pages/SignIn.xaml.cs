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
    public partial class SignIn : Page
    {
        public SignIn()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Evento del botón "Crear cuenta".
        /// </summary>
        private void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            string first = txtFirstName.Text;
            string last = txtLastName.Text;
            string email = txtEmail.Text;
            string user = txtUsername.Text;

            MessageBox.Show(
                $"Account created for: {first} {last}\nEmail: {email}\nUsername: {user}",
                "Create Account",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        /// <summary>
        /// Evento del botón "Back" con icono.
        /// </summary>
        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
            else
                MessageBox.Show("No previous page found.");
        }

        /// <summary>
        /// Maneja el clic en el ícono de sonido.
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
