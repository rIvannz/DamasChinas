using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Damas_Chinas_Server.Dtos;
using Damas_Chinas_Server;
namespace Damas_Chinas_Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class MensajeriaService : IMensajeriaService
    {
 
        private static Dictionary<string, IMensajeriaCallback> clientes = new Dictionary<string, IMensajeriaCallback>();

     
        private MensajesRepository _repo = new MensajesRepository();

    
        public void RegistrarCliente(string username)
        {
            var callback = OperationContext.Current.GetCallbackChannel<IMensajeriaCallback>();
            if (!clientes.ContainsKey(username))
                clientes[username] = callback;
        }

       
        public void EnviarMensaje(Mensaje mensaje)
        {
            
            int idRemitente = mensaje.IdUsuario;
            int idDestino = _repo.ObtenerIdPorUsername(mensaje.UsernameDestino);

            _repo.GuardarMensaje(idRemitente, idDestino, mensaje.Texto);

            if (clientes.ContainsKey(mensaje.UsernameDestino))
            {
                try
                {
                    clientes[mensaje.UsernameDestino].RecibirMensaje(mensaje);
                }
                catch
                {
                    clientes.Remove(mensaje.UsernameDestino);
                }
            }
        }

        public Mensaje[] ObtenerHistorialMensajes(int idUsuario, string usernameDestino)
        {
            return _repo.ObtenerHistorialPorUsername(idUsuario, usernameDestino).ToArray();
        }
    }
}
