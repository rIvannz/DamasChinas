using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using System.ServiceModel;

namespace DamasChinas_Server.Interfaces
{
    [ServiceContract]
    public interface ISingInService
    {
        [OperationContract]
        OperationResult ValidateUserData(UserDto userDto);

 
        [OperationContract]
        OperationResult RequestVerificationCode(string email, string cultureCode);

     
        [OperationContract]
        OperationResult CreateUser(UserDto userDto, string code, string cultureCode);
    }
}
