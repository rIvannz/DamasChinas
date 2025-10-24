namespace Damas_Chinas_Server
{
    public class LoginService : IILoginService
    {
        public LoginResult ValidateLogin(string username, string password)
        {
            var repo = new RepositoryUsers();
            return repo.GetLoginResult(username, password);
        }
    }
}
