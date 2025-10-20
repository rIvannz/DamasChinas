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
        // Diccionario de clientes conectados: clave = username del cliente, valor = callback
        private static Dictionary<string, IMensajeriaCallback> clientes = new Dictionary<string, IMensajeriaCallback>();

        // Repositorio para acceder a la BD
        private MensajesRepository _repo = new MensajesRepository();

        // Registrar un cliente cuando se conecta
        public void RegistrarCliente(string username)
        {
            var callback = OperationContext.Current.GetCallbackChannel<IMensajeriaCallback>();
            if (!clientes.ContainsKey(username))
                clientes[username] = callback;
        }

        // Enviar mensaje a un cliente específico y guardar en BD
        public void EnviarMensaje(Mensaje mensaje)
        {
            // Obtener IDs de remitente y destinatario desde usernames
            int idRemitente = mensaje.IdUsuario;
            int idDestino = _repo.ObtenerIdPorUsername(mensaje.UsernameDestino);

            // Guardar mensaje en BD
            _repo.GuardarMensaje(idRemitente, idDestino, mensaje.Texto);

            // Enviar en tiempo real si el destinatario está conectado
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

        // Obtener historial de chat entre dos usuarios por username
        public Mensaje[] ObtenerHistorialMensajes(int idUsuario, string usernameDestino)
        {
            // Simplemente convertir a array lo que devuelve el repo
            return _repo.ObtenerHistorialPorUsername(idUsuario, usernameDestino).ToArray();
        }
    }
}
