using Damas_Chinas_Server.Dtos;
using System.ServiceModel;

namespace Damas_Chinas_Server
{
	[ServiceContract(CallbackContract = typeof(IMensajeriaCallback))]
	public interface IMensajeriaService
	{
		[OperationContract(IsOneWay = true)]
		void SendMessage(Mensaje message);

		[OperationContract(IsOneWay = true)]
		void RegisterClient(string username);

		// Method to get the history between the client and a recipient
		[OperationContract]
		Mensaje[] GetMessageHistory(int clientUserId, string destinationUsername);
	}
}
