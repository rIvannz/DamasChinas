using DamasChinas_Server.Dtos;
using System.ServiceModel;

using DamasChinas_Server.Contracts;

namespace DamasChinas_Server.Interfaces
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

