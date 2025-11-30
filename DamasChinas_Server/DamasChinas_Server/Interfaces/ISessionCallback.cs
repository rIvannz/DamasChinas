using System.ServiceModel;

namespace DamasChinas_Server.Interfaces
{
    /// <summary>
    /// Eventos transversales de sesión (presencia).
    /// </summary>
    [ServiceContract]
    public interface ISessionCallback
    {
        [OperationContract(IsOneWay = true)]
        void SessionExpired();

        [OperationContract(IsOneWay = true)]
        void PlayerDisconnected(string nickname);

        [OperationContract(IsOneWay = true)]
        void PlayerConnected(string nickname);

        [OperationContract(IsOneWay = true)]
        void PlayerInGame(string username);

        [OperationContract(IsOneWay = true)]
        void PlayerLeftGame(string username);

    }
}
