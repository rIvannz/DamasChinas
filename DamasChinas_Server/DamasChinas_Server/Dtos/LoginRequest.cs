namespace DamasChinas_Server.Dtos
{
	public class LoginRequest
	{
		public string Username { get; set; }
		public string Password { get; set; }
	}

	public class LoginResult
	{
		public int IdUsuario { get; set; }
		public string Username { get; set; }
		public bool Success { get; set; }
	}
}
