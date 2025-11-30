using DamasChinas_Server.Dtos;
using System.ServiceModel;

namespace DamasChinas_Server.Interfaces
{
    [ServiceContract(CallbackContract = typeof(ILoginCallback), SessionMode = SessionMode.Required)]
    public interface ILoginService
    {
        [OperationContract(IsOneWay = true)]
        void Login(LoginRequest loginRequest);
    }
}
