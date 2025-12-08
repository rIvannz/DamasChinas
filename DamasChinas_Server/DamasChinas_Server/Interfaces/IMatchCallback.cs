using System.ServiceModel;
using DamasChinas_Server.Dtos;

namespace DamasChinas_Server.Interfaces
{
    [ServiceContract]
    public interface IMatchCallback
    {
        // Notifica a los jugadores que la partida terminó
        [OperationContract(IsOneWay = true)]
        void OnMatchFinished(MatchResultDto result);
    }
}
