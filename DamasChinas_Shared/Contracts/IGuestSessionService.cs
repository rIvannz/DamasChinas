using System.ServiceModel;

namespace DamasChinas_Shared.Contracts
{
    [ServiceContract(CallbackContract = typeof(IGuestSessionCallback))]
    public interface IGuestSessionService
    {
        [OperationContract]
        void Subscribe(string guestUsername);

        [OperationContract]
        void Unsubscribe(string guestUsername);
    }

    [ServiceContract]
    public interface IGuestSessionCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnServerMessage(string code);
    }
}
