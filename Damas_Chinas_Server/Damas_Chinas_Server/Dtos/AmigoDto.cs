using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace Damas_Chinas_Server.Dtos
{
	[DataContract]
	public class AmigoDto
	{
		public int IdAmigo { get; set; }

		[DataMember]
		public string Username { get; set; }

		[DataMember]
		public bool EnLinea { get; set; }

		[DataMember]
		public string Avatar { get; set; }
	}
}
