using DamasChinas_Client.UI.FriendServiceProxy;
using DamasChinas_Client.UI.PopUps;
using DamasChinas_Client.UI.Utilities;
using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

namespace DamasChinas_Client.UI.Pages
{
    public partial class AddFriend : Page
    {
        public AddFriend()
        {
            InitializeComponent();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.GoBack();
            }
            catch (Exception)
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    PopupType.Error);
            }
        }

        private void OnSendClick(object sender, RoutedEventArgs e)
        {
            string username = txtFriendUsername.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_EmptyCredentials"),
                    PopupType.Warning);
                return;
            }

            var senderUsername = ClientSession.CurrentProfile.Username;

            try
            {
                var client = new FriendServiceClient();

                bool success = client.SendFriendRequest(senderUsername, username);

                client.Close();

                if (success)
                {
                    MessageBox.Show(
                        "Solicitud enviada correctamente.",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    NavigationService?.GoBack();
                }
            }
            catch (FaultException fault)
            {
                MessageBox.Show(
                    fault.Message,
                    "Solicitud no enviada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $":\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

    }
}
