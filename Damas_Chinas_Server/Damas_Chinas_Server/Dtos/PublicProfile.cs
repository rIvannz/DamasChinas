using System.Runtime.Serialization;

namespace Damas_Chinas_Server.Dtos
{
    [DataContract]
    public class PublicProfile
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Nombre { get; set; }

        [DataMember]

        public string LastName { get; set; }

        [DataMember]
        public string Telefono { get; set; }

        [DataMember]
        public string Correo { get; set; }
    }

}
