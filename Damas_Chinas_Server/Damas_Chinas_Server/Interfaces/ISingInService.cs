using Damas_Chinas_Server.Dtos;
using System.ServiceModel;
using Damas_Chinas_Server.Contracts;


namespace Damas_Chinas_Server
{
	[ServiceContract]
	public interface ISingInService
	{
		[OperationContract]
		OperationResult CreateUser(UserDto userDto);
	}
}
