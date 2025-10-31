using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace DamasChinas_Server.Dtos
{
	[DataContract]
	public class FriendDto
	{
		public int IdFriend { get; set; }

		[DataMember]
		public string Username { get; set; }

		[DataMember]
		public bool ConnectionState { get; set; }

		[DataMember]
		public string Avatar { get; set; }
	}
}
