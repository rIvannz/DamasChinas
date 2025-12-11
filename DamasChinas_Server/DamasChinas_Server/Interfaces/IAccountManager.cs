using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using System.ServiceModel;

namespace DamasChinas_Server.Interfaces
{
    [ServiceContract]
    public interface IAccountManager
    {
        [OperationContract]
        PublicProfile GetPublicProfile(int idUser);

        [OperationContract]
        PublicFriendProfile GetFriendPublicProfile(string username);

        [OperationContract]
        OperationResult ChangeUsername(string username, string newUsername);

        [OperationContract]
        OperationResult ChangePassword(string email, string newPassword);

        [OperationContract]
        OperationResult ChangeAvatar(int idUser, string avatarFile);

        [OperationContract]
        OperationResult ChangeSocialUrl(int idUser, string socialUrl);

        [OperationContract]
        OperationResult RequestPasswordChangeCode(string email);

        [OperationContract]
        OperationResult ConfirmPasswordChange(string email, string code, string newPassword);
    }
}
