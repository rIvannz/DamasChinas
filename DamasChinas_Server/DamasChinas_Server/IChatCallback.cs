using DamasChinas_Server.Dtos;
using System.ServiceModel;

namespace DamasChinas_Server
{
    [ServiceContract]
    public interface IChatCallback
	{
		[OperationContract(IsOneWay = true)]
		void ReceiveMessage(Message message);
	}
}
