using System;
using System.Runtime.Serialization;

[DataContract]
public class Mensaje
{
	[DataMember]
	public int IdUsuario { get; set; }

	[DataMember]
	public string UsernameDestino { get; set; }

	[DataMember]
	public int IdUsuarioDestino { get; set; }

	[DataMember]
	public string Texto { get; set; }

	[DataMember]
	public DateTime FechaEnvio { get; set; }
}
