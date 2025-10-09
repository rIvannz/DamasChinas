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


namespace DamasChinas_Client
{
    public partial class MenuRegisteredPlayer : Page
    {
        public MenuRegisteredPlayer()
        {
            InitializeComponent();
        }

        // ====== EVENTOS DE BOTONES ======

        private void OnAvatarClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ProfilePlayer());
        }


        private void OnCreateGameClick(object sender, RoutedEventArgs e)
        {
            // TODO: Navegar a la página de creación de partida
            MessageBox.Show("Create Game clicked");
        }

        private void OnJoinPartyClick(object sender, RoutedEventArgs e)
        {
            // TODO: Navegar a la página de unión de partida
            MessageBox.Show("Join Party clicked");
        }

        private void OnHowToPlayClick(object sender, RoutedEventArgs e)
        {
            // TODO: Mostrar reglas o tutorial
            MessageBox.Show("How to Play clicked");
        }

        private void OnStatisticsClick(object sender, RoutedEventArgs e)
        {
            // TODO: Abrir estadísticas del jugador
            MessageBox.Show("Statistics clicked");
        }

        private void OnFriendsClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new DamasChinas_Client.Friends());
        }


        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ConfiSound());
        }


        /// <summary>
        /// Handles the click event for the "Language" icon.
        /// Navigates to the SelectLanguage page.
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

