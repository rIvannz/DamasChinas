using System.ServiceModel;
using Damas_Chinas_Server.Dtos;
using Damas_Chinas_Server.Interfaces;

namespace Damas_Chinas_Server
{
	[ServiceContract(CallbackContract = typeof(ILoginCallback), SessionMode = SessionMode.Required)]
	public interface ILoginService
	{
		[OperationContract(IsOneWay = true)]
		void Login(LoginRequest loginRequest);
	}

	public interface ILoginCallback : ISessionCallback
	{
		[OperationContract(IsOneWay = true)]
		void OnLoginSuccess(PublicProfile profile);

		[OperationContract(IsOneWay = true)]
		void OnLoginError(string message);
	}
}
