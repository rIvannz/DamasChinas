using System.Runtime.Serialization;

[DataContract]
public class PublicProfile
{
    [DataMember]
    public int IdUser { get; set; }  

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

    [DataMember]
    public string AvatarFile { get; set; }

    [DataMember]
    public int MatchesPlayed { get; set; }

    [DataMember]
    public int Wins { get; set; }

    [DataMember]
    public int Loses { get; set; }
}
