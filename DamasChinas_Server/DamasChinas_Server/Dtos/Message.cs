using System;
using System.Runtime.Serialization;

namespace DamasChinas_Server.Dtos
{
    [DataContract]
    public class Message
    {
        [DataMember]
        public string UsarnameSender { get; set; }

        [DataMember]
        public string DestinationUsername { get; set; }

        public int IdDestinationUsername { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public DateTime SendDate { get; set; }
    }
}
