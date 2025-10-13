using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;


namespace Damas_Chinas_Server
{
    [ServiceContract(CallbackContract = typeof(IUsuarioCallback))]
    public interface ISingInService
    {
        [OperationContract]
        void CrearUsuario(string nombre, string apellido, string correo, string password, string username);
    }

    public interface IUsuarioCallback
    {
        [OperationContract]
        void UsuarioCreado(string mensaje);

        [OperationContract]
        void ErrorCreandoUsuario(string mensaje);
    }
}
