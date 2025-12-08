using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamasChinas_Server.logic
{
    public interface IRepositoryUsers
    {
         int GetUserIdByUsername(string username);

    }
}
