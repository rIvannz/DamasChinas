using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamasChinas_Client.UI.Models
{
    public class LobbySummary
    {
        public string Code { get; set; }
        public string HostUsername { get; set; }
        public string PlayerCount { get; set; }
        public string IsPrivate { get; set; }

        public override string ToString()
        {
            // Esto asegura que nunca se muestre el tipo del objeto en la UI
            return $"{Code} - {HostUsername} ({PlayerCount})";
        }
    }
}

