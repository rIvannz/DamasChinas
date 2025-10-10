using DamasChinas_Client.Pages;
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
    /// Interaction logic for MenuRegisteredPlayer.xaml.
    /// Represents the main menu for registered players.
    /// </summary>
    public partial class MenuRegisteredPlayer : Page
    {
        public MenuRegisteredPlayer()
        {
            InitializeComponent();
        }

        // ====== EVENTOS DE BOTONES ======

        /// <summary>
        /// Maneja el clic en el avatar para abrir el perfil del jugador.
        /// </summary>
        private void OnAvatarClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ProfilePlayer());
        }

        /// <summary>
        /// Maneja el clic en "Create Game".
        /// </summary>
        private void OnCreateGameClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Create Game clicked", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            // TODO: Navegar a la página de creación de partida
        }

        /// <summary>
        /// Maneja el clic en "Join Party".
        /// </summary>
        private void OnJoinPartyClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Join Party clicked", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            // TODO: Navegar a la página de unión de partida
        }

        /// <summary>
        /// Maneja el clic en "How to Play".
        /// </summary>
        private void OnHowToPlayClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("How to Play clicked", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            // TODO: Mostrar reglas o tutorial
        }

        /// <summary>
        /// Maneja el clic en "Statistics".
        /// </summary>
        private void OnStatisticsClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Statistics clicked", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            // TODO: Abrir estadísticas del jugador
        }

        /// <summary>
        /// Maneja el clic en el botón "Friends".
        /// </summary>
        private void OnFriendsClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Friends());
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
                MessageBox.Show($"Error while opening language settings: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}