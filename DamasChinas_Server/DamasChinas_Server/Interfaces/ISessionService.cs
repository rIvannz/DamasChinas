using System.ServiceModel;

namespace DamasChinas_Server.Interfaces
{
    [ServiceContract(CallbackContract = typeof(ISessionCallback), SessionMode = SessionMode.Required)]
    public interface ISessionService
    {
        [OperationContract(IsOneWay = true)]
        void Subscribe(string username);

        [OperationContract(IsOneWay = true)]
        void Unsubscribe(string username);
    }
}
