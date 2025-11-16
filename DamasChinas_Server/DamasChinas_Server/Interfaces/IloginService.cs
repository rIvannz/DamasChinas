using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;
using System.ServiceModel;

namespace DamasChinas_Server
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
        void OnLoginError(MessageCode code);

    }
}
