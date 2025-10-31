using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using DamasChinas_Server.Common;

namespace DamasChinas_Server.Contracts
{
  
    [DataContract]
    public sealed class OperationResult
    {
       
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public MessageCode Code { get; set; }

       
        [DataMember]
        public string TechnicalDetail { get; set; }
    }
}

