using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamasChinas_Server.Utilities
{
    public interface IEmailSender
    {
        Task<bool> SendAsync(string receiver, string subject, string body, bool html = true);
    }
}

