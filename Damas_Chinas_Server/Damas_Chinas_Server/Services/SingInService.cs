using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Damas_Chinas_Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class SingInService : ISingInService
    {
        public void CrearUsuario(string nombre, string apellido, string correo, string password, string username)
        {
            var callback = OperationContext.Current.GetCallbackChannel<IUsuarioCallback>();
            try
            {
                // Aquí va tu lógica de creación de usuario
                var repo = new RepositorioUsuarios();
                var usuario = repo.CrearUsuario(nombre, apellido, correo, password, username);

                callback.UsuarioCreado("hola desde el server");
            }
            catch (Exception ex)
            {
                callback.ErrorCreandoUsuario(ex.Message);
            }
        }
    }
}

