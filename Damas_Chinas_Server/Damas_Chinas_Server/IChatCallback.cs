using Damas_Chinas_Server.Dtos;
using System.ServiceModel;

namespace Damas_Chinas_Server
{
	public interface IChatCallback
	{
		[OperationContract(IsOneWay = true)]
		void ReceiveMessage(Message message);
	}
}
