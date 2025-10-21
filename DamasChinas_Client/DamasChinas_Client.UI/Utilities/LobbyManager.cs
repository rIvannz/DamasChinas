using DamasChinas_Client.UI.LobbyServiceProxy;
using System;
using System.ServiceModel;
using System.Threading;



namespace DamasChinas_Client.UI.Utilities

{
    public class LobbyManager : ILobbyServiceCallback
    {
        private readonly LobbyServiceClient _client;
        private readonly SynchronizationContext _uiContext;

        // Eventos para notificar a la UI (suscribibles desde WPF)
        public event Action<LobbyMember> MemberJoined;
        public event Action<int> MemberLeft;
        public event Action<int, string, string, string> MessageReceived;
        public event Action<string> LobbyClosed;
        public event Action<string> GameStarted;

        public Lobby CurrentLobby { get; private set; }
        public int CurrentUserId { get; private set; }
        public string CurrentUsername { get; private set; }

        public LobbyManager()
        {
            // Captura el contexto actual (útil si la UI llama desde WPF o WinForms)
            _uiContext = SynchronizationContext.Current ?? new SynchronizationContext();

            var context = new InstanceContext(this);
            _client = new LobbyServiceClient(context, "NetTcpBinding_ILobbyService");
        }

        // =============================
        //  MÉTODOS DE CONEXIÓN LOBBY
        // =============================

        // Crear lobby
        public Lobby CreateLobby(int userId, string username, bool isPrivate)
        {
            CurrentUserId = userId;
            CurrentUsername = username;
            CurrentLobby = _client.CreateLobby(userId, username, isPrivate);
            return CurrentLobby;
        }

        // Unirse a un lobby
        public Lobby JoinLobby(string code, int userId, string username)
        {
            CurrentUserId = userId;
            CurrentUsername = username;
            CurrentLobby = _client.JoinLobby(code, userId, username);
            return CurrentLobby;
        }

        // Salir del lobby
        public void LeaveLobby()
        {
            if (CurrentLobby != null)
                _client.LeaveLobby(CurrentLobby.Code, CurrentUserId);
        }

        // Enviar mensaje al lobby
        public void SendMessage(string message)
        {
            if (CurrentLobby != null)
                _client.SendLobbyMessage(CurrentLobby.Code, CurrentUserId, CurrentUsername, message);
        }

        // Iniciar partida
        public void StartGame()
        {
            if (CurrentLobby != null)
                _client.StartGame(CurrentLobby.Code);
        }

        // =============================
        //  CALLBACKS DEL SERVIDOR
        // =============================

        void ILobbyServiceCallback.OnMemberJoined(LobbyMember member)
        {
            _uiContext.Post(_ => MemberJoined?.Invoke(member), null);
        }

        void ILobbyServiceCallback.OnMemberLeft(int userId)
        {
            _uiContext.Post(_ => MemberLeft?.Invoke(userId), null);
        }

        void ILobbyServiceCallback.OnMessageReceived(int userId, string username, string message, string utc)
        {
            _uiContext.Post(_ => MessageReceived?.Invoke(userId, username, message, utc), null);
        }

        void ILobbyServiceCallback.OnLobbyClosed(string reason)
        {
            _uiContext.Post(_ => LobbyClosed?.Invoke(reason), null);
        }

        void ILobbyServiceCallback.OnGameStarted(string code)
        {
            _uiContext.Post(_ => GameStarted?.Invoke(code), null);
        }
    }
}






