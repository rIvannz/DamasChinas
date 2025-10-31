using Damas_Chinas_Server.Dtos;
using System.ServiceModel;

using Damas_Chinas_Server.Contracts;

namespace Damas_Chinas_Server.Interfaces
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

