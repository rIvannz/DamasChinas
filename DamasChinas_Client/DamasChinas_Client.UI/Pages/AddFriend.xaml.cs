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
            try
            {
                using (var client = new FriendServiceClient())
                {
                    bool success = client.SendFriendRequest(ClientSession.safeUsername, username);

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
           
                MessageHelper.ShowPopup(fault.Message, PopupType.Warning);
            }
            catch (EndpointNotFoundException)
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage(MessageKeys.ServerUnavailable),
                    PopupType.Error);
            }
            catch (TimeoutException)
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage(MessageKeys.NetworkLatency),
                    PopupType.Error);
            }
            catch (Exception)
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage(MessageKeys.UnknownError),
                    PopupType.Error);
            }
        }
    }
}
