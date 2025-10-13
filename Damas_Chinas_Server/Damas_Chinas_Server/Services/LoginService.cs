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
