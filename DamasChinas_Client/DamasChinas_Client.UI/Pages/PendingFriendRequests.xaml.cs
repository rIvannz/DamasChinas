using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

using DamasChinas_Client.UI.Callbacks;
using DamasChinas_Client.UI.FriendServiceProxy;
using DamasChinas_Client.UI.Utilities;
using DamasChinas_Client.UI.PopUps;

namespace DamasChinas_Client.UI.Pages
{
    public partial class PendingFriendRequests : Page
    {
        public ObservableCollection<PendingRequest> Requests { get; }
            = new ObservableCollection<PendingRequest>();

        public PendingFriendRequests()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            FriendCallbackHandler.FriendRequestReceivedEvent += OnFriendRequestReceived;
            FriendCallbackHandler.FriendRequestAcceptedEvent += OnFriendRequestAccepted;
            FriendCallbackHandler.FriendRemovedEvent += OnFriendRemoved;

            FriendCallbackHandler.FriendListUpdatedEvent += OnFriendListUpdated;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            LoadRequestsFromServer();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            FriendCallbackHandler.FriendRequestReceivedEvent -= OnFriendRequestReceived;
            FriendCallbackHandler.FriendRequestAcceptedEvent -= OnFriendRequestAccepted;
            FriendCallbackHandler.FriendRemovedEvent -= OnFriendRemoved;

            FriendCallbackHandler.FriendListUpdatedEvent -= OnFriendListUpdated;
        }

 
        private static FriendServiceClient CreateTemporaryClient()
        {
            var callback = new FriendCallbackHandler();
            var context = new InstanceContext(callback);
            return new FriendServiceClient(context, "NetTcpBinding_IFriendService");
        }

        private static void CloseClientSafely(FriendServiceClient client)
        {
            try
            {
                if (client.State != CommunicationState.Faulted)
                {
                    client.Close();
                }
                else
                {
                    client.Abort();
                }
            }
            catch
            {
                client.Abort();
            }
        }

  
        private void LoadRequestsFromServer()
        {
            Requests.Clear();

            string currentUsername = ClientSession.CurrentProfile.Username;

            FriendServiceClient client = FriendNotificationManager.GetClient();
            bool temporaryClient = false;

            if (client == null)
            {
                client = CreateTemporaryClient();
                temporaryClient = true;
            }

            try
            {
                var dtos = client.GetFriendRequests(currentUsername);

                foreach (var dto in dtos)
                {
                    Requests.Add(new PendingRequest
                    {
                        Username = dto.Username,
                        Avatar = string.IsNullOrWhiteSpace(dto.Avatar) ? "avatarIcon.png" : dto.Avatar,
                        IsOnline = dto.ConnectionState
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PendingFriendRequests.LoadRequests] {ex.Message}");
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage(MessageKeys.UnknownError),
                    PopupType.Error);
            }
            finally
            {
                if (temporaryClient)
                {
                    CloseClientSafely(client);
                }
            }
        }

        private void OnFriendRequestReceived(string fromUsername)
        {
            Dispatcher.Invoke(() =>
            {
                if (!Requests.Any(r =>
                    r.Username.Equals(fromUsername, StringComparison.OrdinalIgnoreCase)))
                {
                    Requests.Add(new PendingRequest
                    {
                        Username = fromUsername,
                        Avatar = "avatarIcon.png",
                        IsOnline = true
                    });
                }
            });
        }

        private void OnFriendRequestAccepted(string username)
        {
            Dispatcher.Invoke(() =>
            {
                var req = Requests.FirstOrDefault(r =>
                    r.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

                if (req != null)
                {
                    Requests.Remove(req);
                }
            });
        }

        private void OnFriendRemoved(string username)
        {
            Dispatcher.Invoke(() =>
            {
                var req = Requests.FirstOrDefault(r =>
                    r.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

                if (req != null)
                {
                    Requests.Remove(req);
                }
            });
        }

        private void OnFriendListUpdated()
        {
            Dispatcher.Invoke(() => LoadRequestsFromServer());
        }

        private void OnAcceptClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement el && el.DataContext is PendingRequest req)
            {
                string current = ClientSession.CurrentProfile.Username;

                FriendServiceClient client = FriendNotificationManager.GetClient();
                bool temporaryClient = false;

                if (client == null)
                {
                    client = CreateTemporaryClient();
                    temporaryClient = true;
                }

                try
                {
                    var result = client.UpdateFriendRequestStatus(current, req.Username, true);

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
                catch (Exception ex)
                {
                    Debug.WriteLine($"[PendingFriendRequests.Accept] {ex.Message}");
                }
                finally
                {
                    if (temporaryClient)
                    {
                        CloseClientSafely(client);
                    }
                }
            }
        }

        private void OnRejectClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement el && el.DataContext is PendingRequest req)
            {
                string current = ClientSession.CurrentProfile.Username;

                FriendServiceClient client = FriendNotificationManager.GetClient();
                bool temporaryClient = false;

                if (client == null)
                {
                    client = CreateTemporaryClient();
                    temporaryClient = true;
                }

                try
                {
                    var result = client.UpdateFriendRequestStatus(current, req.Username, false);

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
                catch (Exception ex)
                {
                    Debug.WriteLine($"[PendingFriendRequests.Reject] {ex.Message}");
                }
                finally
                {
                    if (temporaryClient)
                    {
                        CloseClientSafely(client);
                    }
                }
            }
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }


        public class PendingRequest
        {
            public string Username { get; set; }
            public string Avatar { get; set; }
            public bool IsOnline { get; set; }
        }
    }
}
