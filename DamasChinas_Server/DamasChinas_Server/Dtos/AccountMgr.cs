using System.Runtime.Serialization;

namespace DamasChinas_Server.Dtos
{
	[DataContract]
	public class UserInfo
	{
		[DataMember]
		public int IdUser { get; set; }

		[DataMember]
		public string Username { get; set; }

		[DataMember]
		public string Email { get; set; }

		[DataMember]
		public string FullName { get; set; }
	}


}
