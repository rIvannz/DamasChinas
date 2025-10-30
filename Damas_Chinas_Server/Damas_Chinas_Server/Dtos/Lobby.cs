using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Damas_Chinas_Server.Dtos
{
	public class Lobby
	{
		public string Code { get; set; }
		public int HostUserId { get; set; }
		public bool IsPrivate { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public List<LobbyMember> Members { get; } = new List<LobbyMember>();

	}

	public class LobbyMember
	{
		public int UserId { get; set; }
		public string Username { get; set; }
		public bool IsHost { get; set; }
	}
}



