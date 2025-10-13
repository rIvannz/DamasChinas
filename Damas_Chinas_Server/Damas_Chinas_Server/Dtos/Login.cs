using System.Runtime.Serialization;

[DataContract]
public class LoginResult
{
    [DataMember]
    public int IdUsuario { get; set; }

    [DataMember]
    public string Username { get; set; }

    [DataMember]
    public bool Success { get; set; }
}
