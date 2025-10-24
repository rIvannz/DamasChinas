using System.ServiceModel;


namespace Damas_Chinas_Server
{
    [ServiceContract]
    public interface ISingInService
    {
        [OperationContract]
        OperationResult CreateUser(string nombre, string lastName, string Email, string password, string username);
    }
}

