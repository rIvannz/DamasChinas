using System.Runtime.Serialization;

namespace DamasChinas_Server.Dtos
{
	[DataContract]
	public class UserDto
	{
		[DataMember]
		public string Name { get; set; }
		
		[DataMember]
		public string LastName { get; set; }
		
		[DataMember]
		public string Email { get; set; }

		[DataMember]
		public string Password { get; set; }

		[DataMember]
		public string Username { get; set; }

	}
}
