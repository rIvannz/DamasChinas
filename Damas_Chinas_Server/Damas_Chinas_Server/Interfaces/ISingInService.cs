using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;


namespace Damas_Chinas_Server
{
    [ServiceContract]
    public interface ISingInService
    {
        [OperationContract]
        ResultadoOperacion CrearUsuario(string nombre, string apellido, string correo, string password, string username);
    }
}

