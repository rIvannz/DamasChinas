using Damas_Chinas_Server.Dtos;
using System.ServiceModel;

namespace Damas_Chinas_Server
{
    [ServiceContract(CallbackContract = typeof(IMensajeriaCallback))]
    public interface IMensajeriaService
    {
        [OperationContract(IsOneWay = true)]
        void EnviarMensaje(Mensaje mensaje);

        [OperationContract(IsOneWay = true)]
        void RegistrarCliente(string username);

        // Método para obtener el historial entre el cliente y un destinatario
        [OperationContract]
        Mensaje[] ObtenerHistorialMensajes(int idUsuarioCliente, string usernameDestino);
    }
}
