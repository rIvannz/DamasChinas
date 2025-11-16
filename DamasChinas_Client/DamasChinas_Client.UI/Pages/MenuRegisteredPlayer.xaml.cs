using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DamasChinas_Client.UI.AccountManagerServiceProxy;
using DamasChinas_Client.UI.Utilities;
using System.Diagnostics;
using System.ServiceModel;


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
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnAvatarClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnAvatarClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_ProfileOpenError"),
                    "error"
                );
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
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnCreateGameClick - Communication] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_ServerUnavailable"),
                    "error"
                );
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnCreateGameClick - Timeout] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NetworkLatency"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnCreateGameClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_CreateLobbyError"),
                    "error"
                );
            }
        }

  

        private void OnJoinPartyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var joinPartyPage = new JoinParty(_userId, _profile.Username);
                NavigationService?.Navigate(joinPartyPage);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnJoinPartyClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnJoinPartyClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_JoinPartyOpenError"),
                    "error"
                );
            }
        }

   

        private void OnHowToPlayClick(object sender, RoutedEventArgs e)
        {
            MessageHelper.ShowPopup(
                MessageTranslator.GetLocalizedMessage("msg_TutorialUnavailable"),
                "info"
            );
        }



        private void OnStatisticsClick(object sender, RoutedEventArgs e)
        {
            MessageHelper.ShowPopup(
                MessageTranslator.GetLocalizedMessage("msg_StatsUnavailable"),
                "info"
            );
        }

        private void OnFriendsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new Friends(_profile.Username));
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnFriendsClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnFriendsClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_FriendsOpenError"),
                    "error"
                );
            }
        }


        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ConfiSound());
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnSoundClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnSoundClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }

     

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new SelectLanguage());
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnLanguageClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnLanguageClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }
    }
}
