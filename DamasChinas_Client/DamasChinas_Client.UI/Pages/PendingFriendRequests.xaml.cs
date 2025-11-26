using DamasChinas_Client.UI.FriendServiceProxy;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

namespace DamasChinas_Client.UI.Pages
{
    public partial class PendingFriendRequests : Page
    {
        public ObservableCollection<PendingRequest> Requests { get; } =
            new ObservableCollection<PendingRequest>();

        public PendingFriendRequests()
        {
            InitializeComponent();
            DataContext = this;

            LoadRequestsFromServer();
        }

        private void LoadRequestsFromServer()
        {
            Requests.Clear();

            string currentUsername = ClientSession.CurrentProfile.Username;

            try
            {
                using (var client = new FriendServiceClient())
                {
                    var dtos = client.GetFriendRequests(currentUsername);

                    foreach (var dto in dtos)
                    {
                        Requests.Add(new PendingRequest
                        {
                            Username = dto.Username,
                            Avatar = dto.Avatar,
                            IsOnline = dto.ConnectionState
                        });
                    }
                }

                if (Requests.Count == 0)
                {
                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage(MessageKeys.NoPendingRequests),
                        PopupType.Info);
                }
            }
            catch (EndpointNotFoundException)
            {
                MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
            }
            catch (TimeoutException)
            {
                MessageHelper.ShowPopup(MessageKeys.NetworkLatency, PopupType.Error);
            }
            catch (FaultException fault)
            {
                MessageHelper.ShowPopup(fault.Message, PopupType.Warning);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PendingFriendRequests.LoadRequests - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage(MessageKeys.UnknownError),
                    PopupType.Error);
            }

        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new Friends(ClientSession.CurrentProfile.Username));
            }
            catch (InvalidOperationException)
            {
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }

        private void OnAcceptClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element &&
                element.DataContext is PendingRequest req)
            {
                string currentUsername = ClientSession.CurrentProfile.Username;

                try
                {
                    using (var client = new FriendServiceClient())
                    {
                        bool success = client.UpdateFriendRequestStatus(
                            receiverUsername: currentUsername,
                            senderUsername: req.Username,
                            accept: true);

                        if (success)
                        {
                            Requests.Remove(req);

                            MessageHelper.ShowPopup(
                                MessageTranslator.GetLocalizedMessage(MessageKeys.FriendRequestAccepted),
                                PopupType.Success);
                        }
                    }
                }
                catch (FaultException fault)
                {
                    MessageHelper.ShowPopup(fault.Message, PopupType.Warning);
                }
                catch (Exception)
                {
                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage(MessageKeys.UnknownError),
                        PopupType.Error);
                }
            }
        }

        private void OnRejectClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element &&
                element.DataContext is PendingRequest req)
            {
                string currentUsername = ClientSession.CurrentProfile.Username;

                try
                {
                    using (var client = new FriendServiceClient())
                    {
                        bool success = client.UpdateFriendRequestStatus(
                            receiverUsername: currentUsername,
                            senderUsername: req.Username,
                            accept: false);

                        if (success)
                        {
                            Requests.Remove(req);

                            MessageHelper.ShowPopup(
                                MessageTranslator.GetLocalizedMessage(MessageKeys.FriendRequestRejected),
                                PopupType.Info);
                        }
                    }
                }
                catch (FaultException fault)
                {
                    MessageHelper.ShowPopup(fault.Message, PopupType.Warning);
                }
                catch (Exception)
                {
                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage(MessageKeys.UnknownError),
                        PopupType.Error);
                }
            }
        }

        public class PendingRequest
        {
            public string Username { get; set; }
            public string Avatar { get; set; }
            public bool IsOnline { get; set; }
        }
    }
}
