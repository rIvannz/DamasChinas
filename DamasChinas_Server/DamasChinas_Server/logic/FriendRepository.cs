using DamasChinas_Server.Dtos;
using DamasChinas_Server.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ServiceModel;

namespace DamasChinas_Server
{
    public class FriendRepository
    {
        private const string PendingStatus = "pendiente";

        private readonly RepositoryUsers _userRepo = new RepositoryUsers();

        protected virtual damas_chinasEntities CréateDbContext()
        {
            return new damas_chinasEntities();
        }


        private static void EnsureDifferentUsers(int idUser1, int idUser2)
        {
            if (idUser1 == idUser2)
            {
                throw new FaultException("Un usuario no puede interactuar consigo mismo.");
            }
        }

        private static void EnsureUsersExist(damas_chinasEntities db, int idUser1, int idUser2)
        {
            bool exist1 = db.usuarios.Any(u => u.id_usuario == idUser1);
            bool exist2 = db.usuarios.Any(u => u.id_usuario == idUser2);

            if (!exist1 || !exist2)
            {
                throw new FaultException("Uno o ambos usuarios no existen.");
            }
        }

        private static void EnsureNotFriends(damas_chinasEntities db, int idUserSender, int idUserReciever)
        {
            if (FriendshipExists(db, idUserSender, idUserReciever))
            {
                throw new FaultException("Los usuarios ya son amigos.");
            }
        }

        private static void EnsureFriends(damas_chinasEntities db, int idUserSender, int idUserReciever)
        {
            if (!FriendshipExists(db, idUserSender, idUserReciever))
            {
                throw new FaultException("Los usuarios no son amigos.");
            }
        }

        private static void EnsureNotBlocked(damas_chinasEntities db, int idUserSender, int idUserReciever)
        {
            if (IsBlocked(db, idUserSender, idUserReciever))
            {
                throw new FaultException("La relación está bloqueada entre los usuarios.");
            }
        }

        private static void EnsureNotBlockedSelf(int idUserSender, int idUserReciever)
        {
            if (idUserSender == idUserReciever)
            {
                throw new InvalidOperationException("Un usuario no puede bloquearse a sí mismo.");
            }
        }

        private static void EnsureNoPendingRequest(damas_chinasEntities db, int idUserSender, int idUserReciever)
        {
            if (PendingRequestExists(db, idUserSender, idUserReciever))
            {
                throw new FaultException("Ya existe una solicitud pendiente entre los usuarios.");
            }
        }

        private static void EnsurePendingRequestExists(damas_chinasEntities db, int idUserSender, int idUserReciever)
        {
            if (!PendingRequestExists(db, idUserSender, idUserReciever))
            {
                throw new FaultException("No existe una solicitud pendiente entre los usuarios.");
            }
        }

   
        private static bool FriendshipExists(damas_chinasEntities db, int idUserSender, int idUserReciever)
        {
            return db.amistades.Any(a =>
                (a.id_usuario1 == idUserSender && a.id_usuario2 == idUserReciever) ||
                (a.id_usuario1 == idUserSender && a.id_usuario2 == idUserReciever));
        }

        private static bool IsBlocked(damas_chinasEntities db, int idUserSender, int idUserReciever)
        {
            return db.bloqueos.Any(b =>
                (b.id_bloqueador == idUserSender && b.id_bloqueado == idUserReciever) ||
                (b.id_bloqueador == idUserSender && b.id_bloqueado == idUserReciever));
        }

        private static bool PendingRequestExists(damas_chinasEntities db, int idUserSender, int idUserReciever)
        {
            return db.solicitudes_amistad.Any(s =>
                ((s.id_emisor == idUserSender && s.id_receptor == idUserReciever) ||
                 (s.id_emisor == idUserSender && s.id_receptor == idUserReciever)) &&
                s.estado == PendingStatus);
        }

     
        private static FriendDto MapToFriendDto(usuarios user)
        {
            var profile = user.perfiles.FirstOrDefault();
            string username = profile?.username ?? "N/A";
            bool isOnline = !string.IsNullOrWhiteSpace(username) &&
                            SessionManager.IsOnline(username);

            return new FriendDto
            {
                IdFriend = user.id_usuario,
                Username = username,
                ConnectionState = isOnline,
                Avatar = profile?.imagen_perfil ?? "default.png"
            };
        }

  
        private (int receiverId, int senderId) GetUserIds(string receiverUsername, string senderUsername)
        {
            int receiverId = _userRepo.GetUserIdByUsername(receiverUsername);
            int senderId = _userRepo.GetUserIdByUsername(senderUsername);

            EnsureDifferentUsers(receiverId, senderId);

            return (receiverId, senderId);
        }


        private static void ApplyBlock(damas_chinasEntities db, int blockerId, int blockedId)
        {
            if (IsBlocked(db, blockerId, blockedId))
            {
                throw new InvalidOperationException("El usuario ya está bloqueado.");
            }

            RemoveFriendshipIfExists(db, blockerId, blockedId);
            RemovePendingRequests(db, blockerId, blockedId);

            db.bloqueos.Add(new bloqueos
            {
                id_bloqueador = blockerId,
                id_bloqueado = blockedId,
                fecha_bloqueo = DateTime.Now
            });
        }

        private static void RemoveBlock(damas_chinasEntities db, int blockerId, int blockedId)
        {
            var blockEntry = db.bloqueos.FirstOrDefault(b =>
                b.id_bloqueador == blockerId &&
                b.id_bloqueado == blockedId);

            if (blockEntry == null)
            {
                throw new InvalidOperationException("No existe un bloqueo entre los usuarios.");
            }

            db.bloqueos.Remove(blockEntry);
        }

        private static void RemoveFriendshipIfExists(damas_chinasEntities db, int idUserBlocker, int idUserBlocked)
        {
            var friendship = db.amistades.FirstOrDefault(a =>
                (a.id_usuario1 == idUserBlocker && a.id_usuario2 == idUserBlocked) ||
                (a.id_usuario1 == idUserBlocked && a.id_usuario2 == idUserBlocker));

            if (friendship != null)
            {
                db.amistades.Remove(friendship);
            }
        }

        private static void RemovePendingRequests(damas_chinasEntities db, int idUserRemover, int idUserRemoved)
        {
            var pending = db.solicitudes_amistad
                .Where(s =>
                    (s.id_emisor == idUserRemover && s.id_receptor == idUserRemoved) ||
                    (s.id_emisor == idUserRemoved && s.id_receptor == idUserRemover))
                .ToList();

            if (pending.Any())
            {
                db.solicitudes_amistad.RemoveRange(pending);
            }
        }


        public List<FriendDto> GetFriends(string username)
        {
            int id = _userRepo.GetUserIdByUsername(username);

            using (var db = CréateDbContext())
            {
                var friendships = db.amistades
                    .Include(a => a.usuarios.perfiles)
                    .Include(a => a.usuarios1.perfiles)
                    .Where(a => a.id_usuario1 == id || a.id_usuario2 == id)
                    .ToList();

                return friendships
                    .Select(a =>
                    {
                        var friend = (a.id_usuario1 == id) ? a.usuarios1 : a.usuarios;
                        return MapToFriendDto(friend);
                    })
                    .ToList();
            }
        }

        public List<FriendDto> GetFriendRequests(string username)
        {
            int id = _userRepo.GetUserIdByUsername(username);

            using (var db = CréateDbContext())
            {
                var requests = db.solicitudes_amistad
                    .Include(s => s.usuarios.perfiles)
                    .Where(s => s.id_receptor == id && s.estado == PendingStatus)
                    .ToList();

                return requests.Select(r => MapToFriendDto(r.usuarios)).ToList();
            }
        }

        public bool SendFriendRequest(string senderUsername, string receiverUsername)
        {
            var ids = GetUserIds(senderUsername, receiverUsername);

            using (var db = CréateDbContext())
            {
                EnsureUsersExist(db, ids.senderId, ids.receiverId);
                EnsureNotFriends(db, ids.senderId, ids.receiverId);
                EnsureNotBlocked(db, ids.senderId, ids.receiverId);
                EnsureNoPendingRequest(db, ids.senderId, ids.receiverId);

                db.solicitudes_amistad.Add(new solicitudes_amistad
                {
                    id_emisor = ids.senderId,
                    id_receptor = ids.receiverId,
                    fecha_envio = DateTime.Now,
                    estado = PendingStatus
                });

                db.SaveChanges();
                return true;
            }
        }

        public bool UpdateFriendRequestStatus(string receiverUsername, string senderUsername, bool accept)
        {
            var ids = GetUserIds(receiverUsername, senderUsername);

            using (var db = CréateDbContext())
            {
                EnsurePendingRequestExists(db, ids.receiverId, ids.senderId);

                var request = db.solicitudes_amistad
                    .FirstOrDefault(s =>
                        s.id_emisor == ids.senderId &&
                        s.id_receptor == ids.receiverId &&
                        s.estado == PendingStatus);

                if (request == null)
                {
                    throw new FaultException("No existe la solicitud pendiente.");
                }

                if (accept)
                {
                    EnsureNotBlocked(db, ids.senderId, ids.receiverId);

                    if (!FriendshipExists(db, ids.senderId, ids.receiverId))
                    {
                        db.amistades.Add(new amistades
                        {
                            id_usuario1 = ids.senderId,
                            id_usuario2 = ids.receiverId,
                            fecha_amistad = DateTime.Now
                        });
                    }

                    db.solicitudes_amistad.Remove(request);
                }
                else
                {
                    request.estado = "rechazada";
                    request.fecha_envio = DateTime.Now;
                }

                db.SaveChanges();
                return true;
            }
        }


        public bool UpdateBlockStatus(string blockerUsername, string blockedUsername, bool block)
        {
            var ids = GetUserIds(blockerUsername, blockedUsername);

            using (var db = CréateDbContext())
            {
                EnsureNotBlockedSelf(ids.senderId, ids.receiverId);

                if (block)
                {
                    ApplyBlock(db, ids.senderId, ids.receiverId);
                }
                else
                {
                    RemoveBlock(db, ids.senderId, ids.receiverId);
                }

                db.SaveChanges();
                return true;
            }
        }

        public bool DeleteFriend(string blockerUsername, string blockedUsername)
        {
            var ids = GetUserIds(blockerUsername, blockedUsername);

            using (var db = CréateDbContext())
            {
                EnsureUsersExist(db, ids.senderId, ids.receiverId);
                EnsureFriends(db, ids.senderId, ids.receiverId);

                RemoveFriendshipIfExists(db, ids.senderId, ids.receiverId);

                db.SaveChanges();
                return true;
            }
        }

        public bool DeleteFriendAndBlock(string blockerUsername, string blockedUsername)
        {
            var ids = GetUserIds(blockerUsername, blockedUsername);

            using (var db = CréateDbContext())
            {
                EnsureUsersExist(db, ids.senderId, ids.receiverId);

                EnsureNotBlockedSelf(ids.senderId, ids.receiverId);

                RemoveFriendshipIfExists(db, ids.senderId, ids.receiverId);

                ApplyBlock(db, ids.senderId, ids.receiverId);

                db.SaveChanges();
                return true;
            }
        }

    }
}
