using DamasChinas_Server.Dtos;
using System.ServiceModel;

namespace DamasChinas_Server
{
	public interface IChatCallback
	{
		[OperationContract(IsOneWay = true)]
		void ReceiveMessage(Message message);
	}
}
