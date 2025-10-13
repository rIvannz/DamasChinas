using Damas_Chinas_Server.Dtos;
using System.ServiceModel;

namespace Damas_Chinas_Server
{
    [ServiceContract]
    public interface IAccountManager
    {
        [OperationContract]
        PublicProfile ObtenerPerfilPublico(int idUsuario);

        [OperationContract]
        ResultadoOperacion CambiarUsername(int idUsuario, string nuevoUsername);

        [OperationContract]
        ResultadoOperacion CambiarPassword(int idUsuario, string nuevaPassword);
    }
}