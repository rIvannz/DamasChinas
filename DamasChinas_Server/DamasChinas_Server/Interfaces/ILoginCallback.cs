using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Shared.Contracts.Dtos;
using System.ServiceModel;

namespace DamasChinas_Server.Interfaces
{
    [ServiceContract]
    public interface ILoginCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnLoginSuccess(PublicProfile profile);

        [OperationContract(IsOneWay = true)]
        void OnLoginError(MessageCode code);

        [OperationContract(IsOneWay = true)]
        void OnLoginBanned(BanInfoDto banInfo);
    }
}
