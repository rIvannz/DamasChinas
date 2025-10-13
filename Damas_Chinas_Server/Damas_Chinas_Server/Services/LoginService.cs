using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Damas_Chinas_Server
{
    public class LoginService : IILoginService
    {
        public LoginResult ValidarLogin(string username, string password)
        {
            var repo = new RepositorioUsuarios();
            return repo.ObtenerLoginResult(username, password);
        }
    }
}
