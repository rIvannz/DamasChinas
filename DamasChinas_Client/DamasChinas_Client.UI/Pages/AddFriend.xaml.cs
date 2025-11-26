using DamasChinas_Client.UI.FriendServiceProxy;
using DamasChinas_Client.UI.PopUps;
using DamasChinas_Client.UI.Utilities;
using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

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
            catch (Exception ex)
            {
                Debug.WriteLine($"[AddFriend.OnCancelClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage(MessageKeys.NavigationError),
                    PopupType.Error);
            }
        }

        private void OnSendClick(object sender, RoutedEventArgs e)
        {
            string username = txtFriendUsername.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage(MessageKeys.EmptyCredentials),
                    PopupType.Warning);

                return;
            }

            string senderUsername = ClientSession.CurrentProfile.Username;

            try
            {
                using (var client = new FriendServiceClient())
                {
                    bool success = client.SendFriendRequest(senderUsername, username);

                    if (success)
                    {
                        MessageHelper.ShowPopup(
                            MessageTranslator.GetLocalizedMessage(MessageKeys.FriendRequestSentOk),
                            PopupType.Success);

                        NavigationService?.GoBack();
                    }
                }
            }
            catch (FaultException fault)
            {
                Debug.WriteLine($"[AddFriend.Send - Fault] {fault.Message}");

                // Mensaje enviado desde servidor, ya traducible
                MessageHelper.ShowPopup(fault.Message, PopupType.Warning);
            }
            catch (EndpointNotFoundException ex)
            {
                Debug.WriteLine($"[AddFriend.Send - Endpoint] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage(MessageKeys.ServerUnavailable),
                    PopupType.Error);
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"[AddFriend.Send - Timeout] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage(MessageKeys.NetworkLatency),
                    PopupType.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AddFriend.Send - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage(MessageKeys.UnknownError),
                    PopupType.Error);
            }
        }
    }
}
