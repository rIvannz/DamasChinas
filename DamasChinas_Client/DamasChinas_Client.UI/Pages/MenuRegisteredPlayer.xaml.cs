using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DamasChinas_Client.UI.Utilities;                
using DamasChinas_Client.UI.Pages;                     
using DamasChinas_Client.UI.AccountManagerServiceProxy; 


namespace DamasChinas_Client
{
    /// <summary>
    /// Interaction logic for MenuRegisteredPlayer.xaml.
    /// Represents the main menu for registered players.
    /// </summary>
    public partial class MenuRegisteredPlayer : Page
    {
        private readonly int _idUsuario;
        private readonly string _username;

        public MenuRegisteredPlayer()
        {
            InitializeComponent();
        }

        public MenuRegisteredPlayer(int idUsuario, string username)
            : this()
        {
            _idUsuario = idUsuario;
            _username = username;
            txtUsername.Text = username;
        }

        // ====== EVENTOS DE BOTONES ======

        /// <summary>
        /// Maneja el clic en el avatar para abrir el perfil del jugador.
        /// </summary>
        private void OnAvatarClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new AccountManagerClient();
                var profile = client.GetPublicProfile(_idUsuario); // Llamada al servicio
                NavigationService?.Navigate(new ProfilePlayer(profile, _idUsuario));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener el perfil: " + ex.Message,
                                "Perfil", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCreateGameClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Crear el LobbyManager e invocar el servicio remoto
                var lobbyManager = new LobbyManager();
                var lobby = lobbyManager.CreateLobby(_idUsuario, _username, false);

                // Navegar al PreLobby pasando los datos del lobby recién creado
                var preLobbyPage = new PreLobby(lobby, _username);
                NavigationService?.Navigate(preLobbyPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear la partida: {ex.Message}",
                                "Create Game", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            try
            {
                NavigationService?.Navigate(new Friends(_idUsuario));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir la lista de amigos: " + ex.Message,
                                "Amigos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
