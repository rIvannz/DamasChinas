using Damas_Chinas_Server.Dtos;
using System.ServiceModel;

namespace Damas_Chinas_Server
{
	[ServiceContract]
	public interface IAccountManager
	{
		[OperationContract]
		PublicProfile GetPublicProfile(int idUser);

		[OperationContract]
		OperationResult ChangeUsername(string username, string newUsername);

		[OperationContract]
		OperationResult ChangePassword(string correo, string newPassword);
	}
}
