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
        OperationResult ChangeUsername(int idUser, string newUsername);

        [OperationContract]
        OperationResult ChangePassword(int idUser, string newPassword);
    }
}