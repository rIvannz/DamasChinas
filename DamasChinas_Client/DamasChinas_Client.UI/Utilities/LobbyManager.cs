using DamasChinas_Client.UI.LobbyServiceProxy;
using System;
using System.Linq;
using System.ServiceModel;
using System.Windows;

namespace DamasChinas_Client.UI.Utilities
{

    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public sealed class LobbyManager : ILobbyServiceCallback
    {
        private LobbyServiceClient _client;
        private InstanceContext _context;

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
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                return;
            }

            _context = new InstanceContext(this);
            _client = new LobbyServiceClient(_context, "NetTcpBinding_ILobbyService");
        }

        private void DispatchToUi(Action action)
        {
            Application.Current?.Dispatcher?.Invoke(action);
        }

        public void RegisterUser(string username)
        {
            CurrentUsername = username;
            InitializeClient();
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

            if (_client.State == CommunicationState.Faulted)
            {
                InitializeClient();
            }

            try
            {
                return _client.CreateLobby(username, request);
            }
            catch (Exception)
            {
                return new OperationResult { Success = false, Code = MessageCode.ServerUnavailable };
            }
        }

        public OperationResult JoinLobby(int lobbyCode, string username)
        {
            if (lobbyCode <= 0)
            {
                return new OperationResult { Success = false, Code = MessageCode.LobbyNotFound };
            }

            RegisterUser(username);

            if (_client.State == CommunicationState.Faulted)
            {
                InitializeClient();
            }

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
            catch (Exception)
            {
                return new OperationResult { Success = false, Code = MessageCode.ServerUnavailable };
            }
        }

        public LobbySnapshotDto GetCurrentLobby(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            if (_client.State == CommunicationState.Faulted)
            {
                InitializeClient();
            }

            try
            {
                return _client.GetCurrentLobby(username);
            }
            catch
            {
                return null;
            }
        }

        public OperationResult LeaveLobby()
        {
            if (string.IsNullOrWhiteSpace(CurrentUsername))
            {
                return new OperationResult { Success = false };
            }

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
                _client?.Abort();
            }

            return new OperationResult { Success = true };
        }

        public void StartGame()
        {
            if (_client.State == CommunicationState.Opened)
            {
                _client.StartGame(CurrentUsername);
            }
        }

        public void KickPlayer(string targetUsername)
        {
            if (_client.State == CommunicationState.Opened)
            {
                _client.KickPlayer(CurrentUsername, CurrentLobbyCode, targetUsername);
            }
        }

        public void SendChatMessage(string message)
        {
            try
            {
                if (_client.State == CommunicationState.Opened)
                {
                    _client.SendLobbyMessage(CurrentUsername, CurrentLobbyCode, message);
                }
            }
            catch
            {
                
            }
        }

        public void ReportPlayer(ReportPlayerRequest request)
        {
            if (_client.State == CommunicationState.Opened)
            {
                _client.ReportPlayer(request);
            }
        }

        public LobbySummaryDto[] GetPublicLobbies()
        {
            try
            {
                if (_client.State == CommunicationState.Faulted)
                {
                    InitializeClient();
                }

                return _client.GetPublicLobbies();
            }
            catch
            {
                return Array.Empty<LobbySummaryDto>();
            }
        }

 

        public void OnLobbySnapshot(LobbySnapshotDto snapshot)
        {
            if (snapshot != null)
            {
                CurrentLobbyCode = snapshot.LobbyCode;

                var host = snapshot.Members.FirstOrDefault(m => m.IsHost);
                if (host != null)
                {
                    HostUsername = host.Username;
                }
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

        public void OnBanStatusUpdated(BanInfoDto ban)
        {
            DispatchToUi(() => BanUpdated?.Invoke(ban));
        }

        public void OnChatMessageReceived(string sender, string message, string timestamp)
        {
            DispatchToUi(() => ChatMessageReceived?.Invoke(sender, message, timestamp));
        }
    }
}