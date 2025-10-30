using Damas_Chinas_Server.Dtos;
using System.ServiceModel;

namespace Damas_Chinas_Server
{
	[ServiceContract]
	public interface IMensajeriaCallback
	{
		[OperationContract(IsOneWay = true)]
		void RecibirMensaje(Mensaje mensaje);
	}
}
