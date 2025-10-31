using System.Runtime.Serialization;

namespace Damas_Chinas_Server
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
