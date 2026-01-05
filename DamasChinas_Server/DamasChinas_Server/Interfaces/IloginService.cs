using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using System.ServiceModel;

namespace DamasChinas_Server.Interfaces
{
    [ServiceContract(CallbackContract = typeof(ILoginCallback), SessionMode = SessionMode.Required)]
    public interface ILoginService
    {
        [OperationContract(IsOneWay = false)]
        [FaultContract(typeof(MessageCode))]
        void Login(LoginRequest loginRequest);
    }
}
