using DamasChinas_Server.Dtos;
using System.ServiceModel;

namespace DamasChinas_Server.Interfaces
{
    [ServiceContract(CallbackContract = typeof(IChatCallback))]
    public interface IChatService
    {
        [OperationContract]
        void SendMessage(Message message);

        [OperationContract]
        void RegistrateClient(string username);

        [OperationContract]
        Message[] GetHistoricalMessages(string usernameSender, string usernameRecipient);
    }
}