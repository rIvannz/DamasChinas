using DamasChinas_Client.UI.LobbyServiceProxy;
using System;
using System.Linq;
using System.ServiceModel;
using System.Windows;

namespace DamasChinas_Client.UI.Utilities
{
    public sealed class LobbyManager : ILobbyServiceCallback
    {
        private readonly LobbyServiceClient _client;

        public int CurrentLobbyCode { get; private set; }
        public string CurrentUsername { get; private set; }
        public string HostUsername { get; private set; }

        public event Action<LobbySnapshotDto> SnapshotReceived;
        public event Action<string> Kicked;
        public event Action<string> Closed;
        public event Action<LobbyInvitationDto> InvitationReceived;
        public event Action GameStarting;
        public event Action<BanInfoDto> BanUpdated;

        public LobbyManager()
        {
            var ctx = new InstanceContext(this);
            _client = new LobbyServiceClient(ctx, "NetTcpBinding_ILobbyService");
        }

        // =========================================================
        //  DISPATCH SEGURO
        // =========================================================
        private static void Dispatch(Action action)
        {
            var app = Application.Current;

            if (app?.Dispatcher == null || app.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                app.Dispatcher.BeginInvoke(action);
            }
        }

        public void RegisterUser(string username)
        {
            CurrentUsername = username;
        }

        public void RegisterLobby(int code)
        {
            CurrentLobbyCode = code;
        }

        // =========================================================
        //  OPERACIONES BÁSICAS
        // =========================================================

        public void CreateLobby(string username, CreateLobbyRequest request)
        {
            RegisterUser(username);
            var res = _client.CreateLobby(username, request);

            if (!res.Success)
            {
                throw new Exception(res.Code.ToString());
            }
        }

        public void JoinLobby(int lobbyCode, string username)
        {
            RegisterUser(username);

            var req = new JoinLobbyRequest
            {
                LobbyCode = lobbyCode,
                Username = username
            };

            var res = _client.JoinLobby(req);

            if (!res.Success)
            {
                throw new Exception(res.Code.ToString());
            }
        }

        public LobbySummaryDto[] GetPublicLobbies()
        {
            return _client.GetPublicLobbies();
        }

        public LobbySnapshotDto GetCurrentLobby()
        {
            if (string.IsNullOrWhiteSpace(CurrentUsername))
            {
                return null;
            }

            return _client.GetCurrentLobby(CurrentUsername);
        }

        public void LeaveLobby()
        {
            if (!string.IsNullOrWhiteSpace(CurrentUsername))
            {
                _client.LeaveLobby(CurrentUsername);
            }
        }

        public void KickPlayer(string targetUsername)
        {
            _client.KickPlayer(CurrentUsername, CurrentLobbyCode, targetUsername);
        }

        public void StartGame()
        {
            _client.StartGame(CurrentUsername);
        }

        public void InviteFriend(string friendUsername)
        {
            _client.InviteFriend(CurrentUsername, friendUsername, CurrentLobbyCode);
        }

        public void ReportPlayer(ReportPlayerRequest req)
        {
            _client.ReportPlayer(req);
        }

        // =========================================================
        //  HELPERS DE ALTO NIVEL (OPCIÓN A)
        // =========================================================

        public LobbySnapshotDto CreateLobbyAndGetSnapshot(string username, CreateLobbyRequest request)
        {
            RegisterUser(username);

            var res = _client.CreateLobby(username, request);
            if (!res.Success)
            {
                throw new Exception(res.Code.ToString());
            }

            var snapshot = _client.GetCurrentLobby(username);
            if (snapshot != null)
            {
                CurrentLobbyCode = snapshot.LobbyCode;
                HostUsername = snapshot.Members?.FirstOrDefault(m => m.IsHost)?.Username;
            }

            return snapshot;
        }

        public LobbySnapshotDto JoinLobbyAndGetSnapshot(int lobbyCode, string username)
        {
            RegisterUser(username);

            var req = new JoinLobbyRequest
            {
                LobbyCode = lobbyCode,
                Username = username
            };

            var res = _client.JoinLobby(req);
            if (!res.Success)
            {
                throw new Exception(res.Code.ToString());
            }

            var snapshot = _client.GetCurrentLobby(username);
            if (snapshot != null)
            {
                CurrentLobbyCode = snapshot.LobbyCode;
                HostUsername = snapshot.Members?.FirstOrDefault(m => m.IsHost)?.Username;
            }

            return snapshot;
        }

        // =========================================================
        //  CALLBACKS
        // =========================================================

        public void OnLobbySnapshot(LobbySnapshotDto snapshot)
        {
            Dispatch(() =>
            {
                if (snapshot != null)
                {
                    CurrentLobbyCode = snapshot.LobbyCode;
                    HostUsername = snapshot.Members?
                        .FirstOrDefault(m => m.IsHost)?.Username;
                }

                SnapshotReceived?.Invoke(snapshot);
            });
        }

        public void OnKickedFromLobby(MessageCode reason)
        {
            Dispatch(() => Kicked?.Invoke(reason.ToString()));
        }

        public void OnLobbyClosed(MessageCode reason)
        {
            Dispatch(() => Closed?.Invoke(reason.ToString()));
        }

        public void OnInvitationReceived(LobbyInvitationDto inv)
        {
            Dispatch(() => InvitationReceived?.Invoke(inv));
        }

        public void OnGameStarting()
        {
            Dispatch(() => GameStarting?.Invoke());
        }

        public void OnBanStatusUpdated(BanInfoDto ban)
        {
            Dispatch(() => BanUpdated?.Invoke(ban));
        }
    }
}
