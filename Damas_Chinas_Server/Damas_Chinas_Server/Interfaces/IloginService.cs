using System.ServiceModel;


namespace Damas_Chinas_Server
{
    [ServiceContract]

    public interface IILoginService
    {
        [OperationContract]
        LoginResult ValidarLogin(string usuarioInput, string password);
    }
}
