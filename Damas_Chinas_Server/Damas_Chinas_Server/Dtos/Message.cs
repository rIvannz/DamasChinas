using System;
using System.Runtime.Serialization;

[DataContract]
public class Message
{
    [DataMember]
    public int IdUser { get; set; }

    [DataMember]
    public string DestinationUsername { get; set; }

    [DataMember]
    public int IdDestinationUsername { get; set; }

    [DataMember]
    public string Text { get; set; }

    [DataMember]
    public DateTime SendDate { get; set; }
}
