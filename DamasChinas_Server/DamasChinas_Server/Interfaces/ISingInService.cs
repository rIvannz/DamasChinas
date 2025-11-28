using DamasChinas_Server.Dtos;
using System.ServiceModel;
using DamasChinas_Server.Contracts;


namespace DamasChinas_Server.Interfaces
{
    [ServiceContract]
    public interface ISingInService
    {
        [OperationContract]
        OperationResult ValidateUserData(UserDto userDto);

        [OperationContract]
        OperationResult RequestVerificationCode(string email);

        [OperationContract]
        OperationResult CreateUser(UserDto userDto, string code);
    }
}
