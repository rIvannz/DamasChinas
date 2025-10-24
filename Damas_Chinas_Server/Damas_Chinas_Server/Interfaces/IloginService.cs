using System.ServiceModel;


namespace Damas_Chinas_Server
{
    [ServiceContract]

    public interface IILoginService
    {
        [OperationContract]
        LoginResult ValidateLogin(string usuarioInput, string password);
    }
}
