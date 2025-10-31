using System.Runtime.Serialization;

namespace DamasChinas_Server.Dtos
{
	[DataContract]
	public class PublicProfile
	{
		[DataMember]
		public string Username { get; set; }

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public string LastName { get; set; }

		[DataMember]
		public string SocialUrl { get; set; }

		[DataMember]
		public string Email { get; set; }
	}

}
