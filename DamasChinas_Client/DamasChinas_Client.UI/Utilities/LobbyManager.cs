using DamasChinas_Client.UI.LobbyServiceProxy;
using DamasChinas_Shared.Contracts.Dtos;
using System;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.ServiceModel;
using System.Windows;

namespace DamasChinas_Client.UI.Utilities
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public sealed class LobbyManager : ILobbyServiceCallback
    {
        private const string BindingName = "NetTcpBinding_ILobbyService";

        private LobbyServiceClient _client;

        public int CurrentLobbyCode { get; private set; }
        public string CurrentUsername { get; private set; }
        public string HostUsername { get; private set; }

        public event Action<LobbySnapshotDto> SnapshotReceived;
        public event Action<string> Kicked;
        public event Action<string> Closed;
        public event Action<LobbyInvitationDto> InvitationReceived;
        public event Action GameStarting;
        public event Action<BanInfoDto> BanUpdated;
        public event Action<string, string, string> ChatMessageReceived;

        public LobbyManager()
        {
            InitializeClient();
        }

        private void InitializeClient()
        {
            try
            {
                if (_client != null && _client.State == CommunicationState.Opened)
                    return;

                SafeAbort(_client);

                var _context = new InstanceContext(this);
                _client = new LobbyServiceClient(_context, BindingName);

            }
            catch
            {
                SafeAbort(_client);
                _client = null;
            }
        }


        private void EnsureClientAlive()
        {
            if (_client == null)
            {
                InitializeClient();
                return;
            }

            if (_client.State == CommunicationState.Faulted ||
                _client.State == CommunicationState.Closed ||
                _client.State == CommunicationState.Closing)
            {
                SafeAbort(_client);
                _client = null;
               

                InitializeClient();
            }
        }

    
        public void Reset()
        {
            try { SafeAbort(_client); }
            finally
            {
                _client = null;
                CurrentLobbyCode = 0;
                CurrentUsername = null;
                HostUsername = null;
            }
        }

        private static void SafeAbort(ICommunicationObject obj)
        {
            if (obj == null)
            {
                return;
            };
            try 
            { 
                obj.Abort();
            }
            catch 
            {
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);

            }
        }

  
        private static void DispatchToUi(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;

            if (dispatcher == null)
            {
                action?.Invoke();
                return;
            }

            dispatcher.BeginInvoke(action);
        }

 

        public void RegisterUser(string username)
        {
            CurrentUsername = username;
            EnsureClientAlive();
        }

        public void RegisterLobby(int code)
        {
            CurrentLobbyCode = code;
        }


        public OperationResult CreateLobby(string username, CreateLobbyRequest request)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new OperationResult { Success = false, Code = MessageCode.UsernameEmpty };
            }
            RegisterUser(username);
            EnsureClientAlive();

            try
            {
                return _client.CreateLobby(username, request);
            }
            catch
            {
                SafeAbort(_client);
                return new OperationResult { Success = false, Code = MessageCode.ServerUnavailable };
            }
        }

        public OperationResult JoinLobbyGuest(int lobbyCode, string username)
        {
            if (lobbyCode <= 0)
            {
                return new OperationResult { Success = false, Code = MessageCode.LobbyNotFound };
            }
            if (string.IsNullOrWhiteSpace(username))
            {
                return new OperationResult { Success = false, Code = MessageCode.UsernameEmpty };
            }
            RegisterUser(username);
            EnsureClientAlive();

            var request = new JoinLobbyRequest
            {
                LobbyCode = lobbyCode,
                Username = username
            };

            try
            {
                var result = _client.JoinLobbyGuest(request);

                if (result.Success)
                {
                    CurrentLobbyCode = lobbyCode;
                }
                return result;
            }
            catch
            {
                SafeAbort(_client);
                return new OperationResult { Success = false, Code = MessageCode.ServerUnavailable };
            }
        }

        public OperationResult JoinLobby(int lobbyCode, string username)
        {
            if (lobbyCode <= 0)
            {
                return new OperationResult { Success = false, Code = MessageCode.LobbyNotFound };
            }
            if (string.IsNullOrWhiteSpace(username))
            {
                return new OperationResult { Success = false, Code = MessageCode.UsernameEmpty };
            }
            RegisterUser(username);
            EnsureClientAlive();

            var request = new JoinLobbyRequest
            {
                LobbyCode = lobbyCode,
                Username = username
            };

            try
            {
                var result = _client.JoinLobby(request);

                if (result.Success)
                {
                    CurrentLobbyCode = lobbyCode;
                }
                return result;
            }
            catch
            {
                SafeAbort(_client);
                return new OperationResult { Success = false, Code = MessageCode.ServerUnavailable };
            }
        }

        public LobbySnapshotDto GetCurrentLobby(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }
            RegisterUser(username);
            EnsureClientAlive();

            try
            {
                return _client.GetCurrentLobby(username);
            }
            catch
            {
                SafeAbort(_client);
                return null;
            }
        }

        public OperationResult LeaveLobby()
        {
            if (string.IsNullOrWhiteSpace(CurrentUsername))
            {
                return new OperationResult { Success = false };
            }
            EnsureClientAlive();

            try
            {
                if (_client.State == CommunicationState.Opened)
                {
                    var result = _client.LeaveLobby(CurrentUsername);
                    CurrentLobbyCode = 0;
                    return result;
                }
            }
            catch
            {
                SafeAbort(_client);
            }

            CurrentLobbyCode = 0;
            return new OperationResult { Success = true };
        }

        public OperationResult InviteFriend(string hostUsername, string friendUsername, int lobbyCode)
        {
            EnsureClientAlive();

            try
            {
                return _client.InviteFriend(hostUsername, friendUsername, lobbyCode);
            }
            catch
            {
                SafeAbort(_client);
                return new OperationResult { Success = false, Code = MessageCode.ServerUnavailable };
            }
        }

        public void StartGame()
        {
            EnsureClientAlive();

            try
            {
                if (_client.State == CommunicationState.Opened)
                {
                    _client.StartGame(CurrentUsername);
                }
            }
            catch
            {
                SafeAbort(_client);
            }
        }

        public void KickPlayer(string targetUsername)
        {
            EnsureClientAlive();

            try
            {
                if (_client.State == CommunicationState.Opened)
                {
                    _client.KickPlayer(CurrentUsername, CurrentLobbyCode, targetUsername);
                }
            }
            catch
            {
                SafeAbort(_client);
            }
        }

        public void SendChatMessage(string message)
        {
            EnsureClientAlive();

            try
            {
                if (_client.State == CommunicationState.Opened)
                {
                    _client.SendLobbyMessage(CurrentUsername, CurrentLobbyCode, message);
                }
            }
            catch
            {
                SafeAbort(_client);
            }
        }

        public OperationResult ReportPlayer(ReportPlayerRequest request)
        {
            EnsureClientAlive();

            try
            {
                if (_client.State == CommunicationState.Opened)
                {
                    return _client.ReportPlayer(request);
                }
            }
            catch
            {
                SafeAbort(_client);
            }

            return new OperationResult { Success = false, Code = MessageCode.ServerUnavailable };
        }

        public LobbySummaryDto[] GetPublicLobbies()
        {
            EnsureClientAlive();

            try
            {
                return _client.GetPublicLobbies();
            }
            catch
            {
                SafeAbort(_client);
                return Array.Empty<LobbySummaryDto>();
            }
        }


        public void OnLobbySnapshot(LobbySnapshotDto snapshot)
        {
            if (snapshot != null)
            {
                CurrentLobbyCode = snapshot.LobbyCode;

                var host = snapshot.Members?.FirstOrDefault(m => m.IsHost);
                if (host != null)
                    HostUsername = host.Username;
            }

            DispatchToUi(() => SnapshotReceived?.Invoke(snapshot));
        }

        public void OnKickedFromLobby(MessageCode reason)
        {
            DispatchToUi(() => Kicked?.Invoke(reason.ToString()));
        }

        public void OnLobbyClosed(MessageCode reason)
        {
            DispatchToUi(() => Closed?.Invoke(reason.ToString()));
        }

        public void OnInvitationReceived(LobbyInvitationDto invitation)
        {
            DispatchToUi(() => InvitationReceived?.Invoke(invitation));
        }

        public void OnGameStarting()
        {
            DispatchToUi(() => GameStarting?.Invoke());
        }

        public void OnBanStatusUpdated(BanInfoDto banInfo)
        {
            DispatchToUi(() => BanUpdated?.Invoke(banInfo));
        }

        public void OnChatMessageReceived(string sender, string message, string timestamp)
        {
            DispatchToUi(() => ChatMessageReceived?.Invoke(sender, message, timestamp));
        }
    }
}
