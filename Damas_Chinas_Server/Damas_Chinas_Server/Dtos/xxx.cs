using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

namespace Damas_Chinas_Server
{
    [DataContract]
    public class UsuarioInfo
    {
        [DataMember]
        public int IdUsuario { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Correo { get; set; }

        [DataMember]
        public string NombreCompleto { get; set; }
    }

    [DataContract]
    public class ResultadoOperacion
    {
        [DataMember]
        public bool Exito { get; set; }

        [DataMember]
        public string Mensaje { get; set; }

        [DataMember]
        public UsuarioInfo Usuario { get; set; }
    }

}
