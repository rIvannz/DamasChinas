using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;


namespace Damas_Chinas_Server
{
    [ServiceContract]

    public interface IILoginService
    {
        [OperationContract]
        LoginResult ValidarLogin(string usuarioInput, string password);
    }
}
