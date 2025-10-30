using Damas_Chinas_Server.Dtos;
using System.ServiceModel;

namespace Damas_Chinas_Server
{
	[ServiceContract]
	public interface ISingInService
	{
		[OperationContract]
		OperationResult CreateUser(UserDto userDto);
	}
}
