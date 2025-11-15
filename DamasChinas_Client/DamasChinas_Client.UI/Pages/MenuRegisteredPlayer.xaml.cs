using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DamasChinas_Client.UI.AccountManagerServiceProxy;
using DamasChinas_Client.UI.Utilities;

namespace DamasChinas_Client.UI.Pages
{
	public partial class MenuRegisteredPlayer : Page
	{
		private readonly PublicProfile _profile;
		private readonly int _userId;

                public MenuRegisteredPlayer(PublicProfile profile)
                {
                        InitializeComponent();
                        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
                        _userId = 1;
                        txtUsername.Text = _profile.Username;
                }

        

                private void OnAvatarClick(object sender, RoutedEventArgs e)
                {
                        try
                        {
                                var profilePage = new ProfilePlayer(_profile);
                                NavigationService?.Navigate(profilePage);
			}
			catch (Exception ex)
			{
				ShowError("Error al abrir el perfil: " + ex.Message, "Perfil");
			}
		}

        private void OnCreateGameClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var lobbyManager = new LobbyManager();
                var lobby = lobbyManager.CreateLobby(_userId, _profile.Username, false);

                var preLobbyPage = new PreLobby(lobby, _userId, _profile.Username);
                NavigationService?.Navigate(preLobbyPage);
            }
            catch (Exception ex)
            {
                ShowError($"Error al crear la partida: {ex.Message}", "Create Game");
            }
        }


        private void OnJoinPartyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var joinPartyPage = new JoinParty(_userId, _profile.Username);
                NavigationService?.Navigate(joinPartyPage);
            }
            catch (Exception ex)
            {
                ShowError($"Error while opening join party page: {ex.Message}", "Join Party");
            }
        }


        private void OnHowToPlayClick(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("How to Play clicked", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void OnStatisticsClick(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Statistics clicked", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void OnFriendsClick(object sender, RoutedEventArgs e)
		{
			try
			{
				NavigationService?.Navigate(new Friends(_profile.Username));
			}
			catch (Exception ex)
			{
				ShowError("Error al abrir la lista de amigos: " + ex.Message, "Amigos");
			}
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
				ShowError($"Error while opening language settings: {ex.Message}", "Error");
			}
		}

		private void ShowError(string message, string title = "Error")
		{
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}
