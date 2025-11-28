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
            catch (Exception ex)
            {
                Debug.WriteLine($"[PendingFriendRequests.LoadRequests] {ex.Message}");
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage(MessageKeys.UnknownError),
                    PopupType.Error);
            }

        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new Friends());
            }
            catch
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
                        var result = client.UpdateFriendRequestStatus(
                            currentUsername,
                            req.Username,
                            true);

                        if (!result.Success)
                        {
                            string msg = MessageTranslator.GetLocalizedMessage(result.Code);
                            MessageHelper.ShowPopup(msg, PopupType.Warning);
                            return;
                        }

                        Requests.Remove(req);

                        MessageHelper.ShowPopup(
                            MessageTranslator.GetLocalizedMessage(MessageKeys.FriendRequestAccepted),
                            PopupType.Success);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[PendingFriendRequests.Accept] {ex.Message}");
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
                        var result = client.UpdateFriendRequestStatus(
                            currentUsername,
                            req.Username,
                            false);

                        if (!result.Success)
                        {
                            string msg = MessageTranslator.GetLocalizedMessage(result.Code);
                            MessageHelper.ShowPopup(msg, PopupType.Warning);
                            return;
                        }

                        Requests.Remove(req);

                        MessageHelper.ShowPopup(
                            MessageTranslator.GetLocalizedMessage(MessageKeys.FriendRequestRejected),
                            PopupType.Info);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[PendingFriendRequests.Reject] {ex.Message}");
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
