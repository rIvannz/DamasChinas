using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Damas_Chinas_Server.Dtos
{
    [DataContract]
    public class PublicProfile
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Nombre{ get; set; }

        [DataMember]
       
        public string LastName { get; set; }

        [DataMember]
        public string Telefono { get; set; }

        [DataMember]
        public string Correo { get; set; }
    }

}
