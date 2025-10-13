using System.ServiceModel;


namespace Damas_Chinas_Server
{
    [ServiceContract]
    public interface ISingInService
    {
        [OperationContract]
        ResultadoOperacion CrearUsuario(string nombre, string apellido, string correo, string password, string username);
    }
}

