using DamasChinas_Server.Dtos;
using DamasChinas_Server.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace DamasChinas_Server
{
	public class FriendRepository
	{
        public List<FriendDto> GetFriends(string username)
        {
            var userRepo = new RepositoryUsers();
            int id = userRepo.GetUserIdByUsername(username);

            using (var db = new damas_chinasEntities())
            {
                var friends = db.amistades
                                .Include(a => a.usuarios.perfiles)
                                .Include(a => a.usuarios1.perfiles)
                                .Where(a => a.id_usuario1 == id || a.id_usuario2 == id)
                                .ToList();

                if (friends == null || friends.Count == 0)
                    return new List<FriendDto>();

                var friendships = friends.Select(a =>
                {
                    var friendUser = a.id_usuario1 == id ? a.usuarios1 : a.usuarios;

                    var profile = friendUser.perfiles.FirstOrDefault();

                    string friendUsername = profile?.username ?? "N/A";
                    bool isOnline = !string.IsNullOrWhiteSpace(friendUsername) &&
                                    SessionManager.IsOnline(friendUsername);

                    return new FriendDto
                    {
                        IdFriend = friendUser.id_usuario,
                        Username = friendUsername,
                        ConnectionState = isOnline,
                        Avatar = profile?.imagen_perfil ?? "default.png"
                    };
                }).ToList();

                return friendships;
            }
        }


        public bool AddFriend(int idUsuario1, int idUsuario2)
		{
			if (idUsuario1 == idUsuario2)
					{
					throw new Exception("Un usuario no puede agregarse a sí mismo.");
				}

			using (var db = new damas_chinasEntities())
			{
				var existe1 = db.usuarios.Any(u => u.id_usuario == idUsuario1);
				var existe2 = db.usuarios.Any(u => u.id_usuario == idUsuario2);

				if (!existe1 || !existe2)
				{
					throw new Exception("Uno o ambos usuarios no existen.");
				}
				bool yaSonAmigos = db.amistades.Any(a =>
					(a.id_usuario1 == idUsuario1 && a.id_usuario2 == idUsuario2) ||
					(a.id_usuario1 == idUsuario2 && a.id_usuario2 == idUsuario1)
				);

				if (yaSonAmigos)
				{
					throw new Exception("Estos usuarios ya son amigos.");
				}
				var nuevaAmistad = new amistades
				{
					id_usuario1 = idUsuario1,
					id_usuario2 = idUsuario2,
					fecha_amistad = DateTime.Now
				};

				db.amistades.Add(nuevaAmistad);
				db.SaveChanges();

				return true;
			}
		}

		public bool DeleteFriend(int idUsuario1, int idUsuario2)
		{
			using (var db = new damas_chinasEntities())
			{
				var amistad = db.amistades.FirstOrDefault(a =>
					(a.id_usuario1 == idUsuario1 && a.id_usuario2 == idUsuario2) ||
					(a.id_usuario1 == idUsuario2 && a.id_usuario2 == idUsuario1)
				);

				if (amistad == null)
				{
					throw new Exception("No existe una amistad entre estos usuarios.");
				}

				db.amistades.Remove(amistad);
				db.SaveChanges();
				return true;
			}
		}
	}
}
