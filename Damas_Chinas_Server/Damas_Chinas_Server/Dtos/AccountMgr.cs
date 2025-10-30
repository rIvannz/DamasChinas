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

	[DataContract]
	public class OperationResult
	{
		[DataMember]
		public bool Succes { get; set; }

		[DataMember]
		public string Messaje { get; set; }

		[DataMember]
		public UserInfo User { get; set; }
	}

}
