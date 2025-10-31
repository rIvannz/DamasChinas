using DamasChinas_Server.Dtos;
using System.ServiceModel;

namespace DamasChinas_Server
{
	[ServiceContract(CallbackContract = typeof(IChatCallback))]
	public interface IChatService
	{
		[OperationContract(IsOneWay = true)]
		void SendMessage(Message message);

		[OperationContract(IsOneWay = true)]
		void RegistrateClient(string username);

		[OperationContract]
		Message[] GetHistoricalMessages(string usernameSender, string usernameRecipient);
	}
}
