using DamasChinas_Server.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DamasChinas_Server
{
	public class FriendService : IFriendService
	{
		private readonly FriendRepository repo = new FriendRepository();

		public List<FriendDto> GetFriends(string username)
		{
			return repo.GetFriends(username);
		}
	}
}
