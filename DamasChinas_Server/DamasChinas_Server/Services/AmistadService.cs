using Damas_Chinas_Server.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Damas_Chinas_Server
{
	public class AmistadService : IAmistadService
	{
		private readonly RepositorioAmistades repo = new RepositorioAmistades();

		public List<AmigoDto> GetFriends(int userId)
		{
			return repo.ObtenerAmigos(userId);
		}
	}
}
