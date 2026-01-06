using DamasChinas_Server;
using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.logic;
using DamasChinas_Server.logic;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DamasChinas_Pruebas
{
    public class FriendRepositoryTests
    {
        [Fact]
        public void EnsureDifferentUsers_SameIds_ThrowsUserValidationError()
        {
             
            var method = typeof(FriendRepository)
                .GetMethod("EnsureDifferentUsers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            int sameId = 5;

             
            Assert.NotNull(method);
            var ex = Record.Exception(() => method.Invoke(null, new object[] { sameId, sameId }));


            Assert.True(
             ex.InnerException is RepositoryValidationException validationEx &&
            validationEx.Code == MessageCode.UserValidationError
         );

        }

        [Fact]
        public void EnsureDifferentUsers_DifferentIds_DoesNotThrow()
        {
            var method = typeof(FriendRepository)
                .GetMethod("EnsureDifferentUsers", BindingFlags.NonPublic | BindingFlags.Static);

            Assert.NotNull(method);

            try
            {
                method.Invoke(null, new object[] { 1, 2 });
            }
            catch (TargetInvocationException ex)
            {
                Assert.Null(ex.InnerException);
            }
        }


        [Fact]
        public void EnsureUsersExist_UserMissing_ThrowsUserNotFound()
        {
             
            var method = typeof(FriendRepository)
                .GetMethod("EnsureUsersExist", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsureUsersExist no encontrado.");

            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios).Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

             
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object!, 1, 99 })
            );
            Assert.NotNull(exception);



            Assert.True(
            exception is not null &&
            exception.InnerException is RepositoryValidationException validationEx &&
            validationEx.Code == MessageCode.UserNotFound

            );
        }


        [Fact]
        public void EnsureUsersExist_BothExist_DoesNotThrow()
        {
             
            var method = typeof(FriendRepository)
                .GetMethod("EnsureUsersExist", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsureUsersExist no encontrado.");

            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1 },
        new usuarios { id_usuario = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios).Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

             
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object!, 1, 2 })
            );

             
            Assert.Null(exception);
        }

        [Fact]
        public void EnsureNotFriends_AlreadyFriends_ThrowsAlreadyFriends()
        {
             
            var method = typeof(FriendRepository)
                .GetMethod("EnsureNotFriends", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsureNotFriends no encontrado.");

            var amistades = new List<amistades>
    {
        new amistades { id_usuario1 = 1, id_usuario2 = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades).Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

             
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object!, 1, 2 })
            );


            Assert.True(
        exception is not null &&
        exception.InnerException is RepositoryValidationException repoException &&
        repoException.Code == MessageCode.AlreadyFriends
           );

        }

        [Fact]
        public void EnsureNotFriends_NoFriendship_DoesNotThrow()
        {
             
            var method = typeof(FriendRepository)
                .GetMethod("EnsureNotFriends", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsureNotFriends no encontrado.");

            var amistades = new List<amistades>(); 

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades).Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

             
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object!, 1, 2 })
            );

             
            Assert.Null(exception);
        }

        [Fact]
        public void EnsureFriends_NoFriendship_ThrowsFriendsLoadError()
        {
             
            var method = typeof(FriendRepository)
                .GetMethod("EnsureFriends", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsureFriends no encontrado.");

            var amistades = new List<amistades>(); 

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades)
                .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

             
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object!, 1, 2 })
            );


            Assert.True(
             exception is not null &&
             exception.InnerException is RepositoryValidationException repoException &&
             repoException.Code == MessageCode.FriendsLoadError
         );

        }

        [Fact]
        public void EnsureFriends_FriendshipExists_DoesNotThrow()
        {
             
            var method = typeof(FriendRepository)
                .GetMethod("EnsureFriends", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsureFriends no encontrado.");

            var amistades = new List<amistades>
    {
        new amistades { id_usuario1 = 1, id_usuario2 = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades)
                .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

             
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object!, 1, 2 })
            );

             
            Assert.Null(exception);
        }

        [Fact]
        public void EnsureNotBlocked_IsBlocked_ThrowsFriendsLoadError()
        {
             
            var method = typeof(FriendRepository)
                .GetMethod("EnsureNotBlocked", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsureNotBlocked no encontrado.");

            var bloqueos = new List<bloqueos>
    {
        new bloqueos { id_bloqueador = 1, id_bloqueado = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.bloqueos)
                .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

             
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object!, 1, 2 })
            );


            Assert.True(
            exception is not null &&
            exception.InnerException is RepositoryValidationException repoException &&
            repoException.Code == MessageCode.FriendsLoadError
);

        }

        [Fact]
        public void EnsureNotBlocked_NoBlock_DoesNotThrow()
        {
             
            var method = typeof(FriendRepository)
                .GetMethod("EnsureNotBlocked", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsureNotBlocked no encontrado.");

            var bloqueos = new List<bloqueos>();

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.bloqueos)
                .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

             
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object!, 1, 2 })
            );

             
            Assert.Null(exception);
        }

        [Fact]
        public void EnsureNotBlockedSelf_SameUser_ThrowsUserValidationError()
        {
             
            var method = typeof(FriendRepository)
                .GetMethod("EnsureNotBlockedSelf", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsureNotBlockedSelf no encontrado.");

             
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { 5, 5 }) 
            );


            Assert.True(
            exception is not null &&
            exception.InnerException is RepositoryValidationException repoException &&
            repoException.Code == MessageCode.UserValidationError
            );

        }

        [Fact]
        public void EnsureNotBlockedSelf_DifferentUsers_DoesNotThrow()
        {
             
            var method = typeof(FriendRepository)
                .GetMethod("EnsureNotBlockedSelf", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsureNotBlockedSelf no encontrado.");

             
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { 1, 2 })
            );

             
            Assert.Null(exception);
        }

        [Fact]
        public void EnsureNoPendingRequest_PendingRequestExists_ThrowsFriendRequestAlreadyPending()
        {
             
            var solicitudes = new List<solicitudes_amistad>
    {
        new solicitudes_amistad
        {
            id_emisor = 1,
            id_receptor = 2,
            estado = "pendiente"
        }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            var method = typeof(FriendRepository)
                .GetMethod("EnsureNoPendingRequest", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsureNoPendingRequest no encontrado.");

             
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
            );


            Assert.True(
            exception is not null &&
            exception.InnerException is RepositoryValidationException repoException &&
            repoException.Code == MessageCode.FriendRequestAlreadyPending
             );

        }



        [Fact]
        public void EnsureNoPendingRequest_NoPendingRequest_DoesNotThrow()
        {
             
            var solicitudes = new List<solicitudes_amistad>(); 

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            var method = typeof(FriendRepository)
                .GetMethod("EnsureNoPendingRequest", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsureNoPendingRequest no encontrado.");

             
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
            );

             
            Assert.Null(exception);
        }

        [Fact]
        public void PendingRequestExists_NoPendingRequest_ReturnsFalse()
        {
             
            var solicitudes = new List<solicitudes_amistad>();

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            var method = typeof(FriendRepository)
                .GetMethod(
                    "PendingRequestExists",
                    BindingFlags.NonPublic | BindingFlags.Static
                )
                ?? throw new Exception("Método PendingRequestExists no encontrado.");

             
            var result = (bool)method.Invoke(
                null,
                new object[] { mockDb.Object, 1, 2 }
            )!;

             
            Assert.False(result);
        }


        [Fact]
        public void PendingRequestExists_PendingRequestExists_ReturnsTrue()
        {
             
            var solicitudes = new List<solicitudes_amistad>
    {
        new solicitudes_amistad
        {
            id_emisor = 1,
            id_receptor = 2,
            estado ="pendiente"
        }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            var method = typeof(FriendRepository)
                .GetMethod(
                    "PendingRequestExists",
                    BindingFlags.NonPublic | BindingFlags.Static
                )
                ?? throw new Exception("Método PendingRequestExists no encontrado.");

             
            var result = (bool)method.Invoke(
                null,
                new object[] { mockDb.Object, 1, 2 }
            )!;

             
            Assert.True(result);
        }


        [Fact]
        public void FriendshipExists_WhenFriendshipExists_ReturnsTrue()
        {
             
            var amistades = new List<amistades>
    {
        new amistades { id_usuario1 = 1, id_usuario2 = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

            var method = typeof(FriendRepository)
                .GetMethod("FriendshipExists", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método FriendshipExists no encontrado.");

             
            var result = (bool)method.Invoke(null, new object[] { mockDb.Object, 1, 2 })!;

             
            Assert.True(result);
        }

        [Fact]
        public void FriendshipExists_WhenFriendshipDoesNotExist_ReturnsFalse()
        {
             
            var amistades = new List<amistades>(); 

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

            var method = typeof(FriendRepository)
                .GetMethod("FriendshipExists", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método FriendshipExists no encontrado.");

             
            var result = (bool)method.Invoke(null, new object[] { mockDb.Object, 1, 2 })!;

             
            Assert.False(result);
        }

        [Fact]
        public void IsBlocked_WhenUserIsBlocked_ReturnsTrue()
        {
             
            var bloqueos = new List<bloqueos>
    {
        new bloqueos { id_bloqueador = 1, id_bloqueado = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            var method = typeof(FriendRepository)
                .GetMethod("IsBlocked", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método IsBlocked no encontrado.");

             
            var result = (bool)method.Invoke(null, new object[] { mockDb.Object, 1, 2 })!;

             
            Assert.True(result);
        }

        [Fact]
        public void IsBlocked_WhenUserIsNotBlocked_ReturnsFalse()
        {
             
            var bloqueos = new List<bloqueos>(); 

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            var method = typeof(FriendRepository)
                .GetMethod("IsBlocked", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método IsBlocked no encontrado.");

             
            var result = (bool)method.Invoke(null, new object[] { mockDb.Object, 1, 2 })!;

             
            Assert.False(result);
        }

        [Fact]
        public void PendingRequestExists_WhenPendingRequestExists_ReturnsTrue()
        {
             
            var solicitudes = new List<solicitudes_amistad>
    {
        new solicitudes_amistad
        {
            id_emisor = 1,
            id_receptor = 2,
            estado = "pendiente"
        }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            var method = typeof(FriendRepository)
                .GetMethod("PendingRequestExists", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método PendingRequestExists no encontrado.");

             
            var result = (bool)method.Invoke(null, new object[] { mockDb.Object, 1, 2 })!;

             
            Assert.True(result);
        }

        [Fact]
        public void PendingRequestExists_WhenNoPendingRequest_ReturnsFalse()
        {
             
            var solicitudes = new List<solicitudes_amistad>(); 

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            var method = typeof(FriendRepository)
                .GetMethod("PendingRequestExists", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método PendingRequestExists no encontrado.");

             
            var result = (bool)method.Invoke(null, new object[] { mockDb.Object, 1, 2 })!;

             
            Assert.False(result);
        }


        [Fact]
        public void MapToFriendDto_WithValidProfile_ReturnsCorrectDto()
        {

            FriendRepository.IsOnlineResolver = _ => false;

            var user = new usuarios
            {
                id_usuario = 10,
                perfiles = new List<perfiles>
        {
            new perfiles
            {
                username = "Seth",
                imagen_perfil = "avatar123.png"
            }
        }
            };

            var method = typeof(FriendRepository)
                .GetMethod(
                    "MapToFriendDto",
                    BindingFlags.NonPublic | BindingFlags.Static
                )!;


            var dto = (FriendDto)method.Invoke(null, new object[] { user })!;


            Assert.True(
            dto.IdFriend == 10 &&
            dto.Username == "Seth" &&
            dto.ConnectionState == false &&
            dto.Avatar == "avatar123.png"
            );
        }

            [Fact]
        public void MapToFriendDto_NoProfile_ThrowsException()
        {
             
            var user = new usuarios
            {
                id_usuario = 55,
                perfiles = new List<perfiles>() 
            };

            var method = typeof(FriendRepository)
                .GetMethod(
                    "MapToFriendDto",
                    BindingFlags.NonPublic | BindingFlags.Static
                )
                ?? throw new Exception("Método MapToFriendDto no encontrado.");

             
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { user })
            );

             
            Assert.NotNull(exception);
        }

        [Fact]
        public void GetUserIds_ValidDifferentUsers_ReturnsCorrectTuple()
        {

            var mockUserRepo = new Mock<IRepositoryUsers>();

            mockUserRepo.Setup(r => r.GetUserIdByUsername("Receiver"))
                        .Returns(10);

            mockUserRepo.Setup(r => r.GetUserIdByUsername("Sender"))
                        .Returns(20);

            var repo = new FriendRepository(mockUserRepo.Object, () => null);

            var method = typeof(FriendRepository)
                .GetMethod("GetUserIds",
                    BindingFlags.NonPublic | BindingFlags.Instance);


            var result = ((int receiverId, int senderId))method.Invoke(
                repo, new object[] { "Receiver", "Sender" }
            );

            Assert.True(
            method is not null &&
            result.receiverId == 10 &&
            result.senderId == 20
            );
        }


        [Fact]
        public void GetUserIds_SameUser_ThrowsUserValidationError()
        {

            var mockUserRepo = new Mock<IRepositoryUsers>();

            mockUserRepo.Setup(r => r.GetUserIdByUsername(It.IsAny<string>()))
                        .Returns(50);
            var repo = new FriendRepository(mockUserRepo.Object, () => null);

            var method = typeof(FriendRepository)
                .GetMethod(
                    "GetUserIds",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );


            Assert.True(
            method is not null &&
            Assert.Throws<TargetInvocationException>(() =>
            method.Invoke(repo, new object[] { "User", "User" })
            ).InnerException is RepositoryValidationException
            );
        }


            [Fact]
        public void ApplyBlock_AlreadyBlocked_ThrowsException()
        {
             
            var bloqueos = new List<bloqueos>
    {
        new bloqueos { id_bloqueador = 1, id_bloqueado = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            var method = typeof(FriendRepository)
                .GetMethod("ApplyBlock", BindingFlags.NonPublic | BindingFlags.Static);

            Assert.True(
            method is not null &&
            Assert.Throws<TargetInvocationException>(() =>
            method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
            ).InnerException is RepositoryValidationException
            );


        }

        [Fact]
        public void ApplyBlock_RemovesFriendshipAndPendingRequests()
        {

            var amistades = new List<amistades>
    {
        new amistades { id_usuario1 = 1, id_usuario2 = 2 }
    };

            var solicitudes = new List<solicitudes_amistad>
    {
        new solicitudes_amistad { id_emisor = 1, id_receptor = 2, estado = "pendiente" }
    };

            var bloqueos = new List<bloqueos>();

            var mockDb = new Mock<damas_chinasEntities>();

            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            var method = typeof(FriendRepository)
                .GetMethod("ApplyBlock", BindingFlags.NonPublic | BindingFlags.Static);


            Assert.True(
    method is not null &&
    Record.Exception(() =>
        method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
    ) == null &&
    !amistades.Any() &&
    !solicitudes.Any()
);
        }

        [Fact]
        public void ApplyBlock_AddsNewBlockEntry()
        {

            var amistades = new List<amistades>();
            var solicitudes = new List<solicitudes_amistad>();
            var bloqueos = new List<bloqueos>();

            var mockDb = new Mock<damas_chinasEntities>();

            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            var method = typeof(FriendRepository)
                .GetMethod("ApplyBlock", BindingFlags.NonPublic | BindingFlags.Static);


            Assert.True(
           method is not null &&
           Record.Exception(() =>
               method.Invoke(null, new object[] { mockDb.Object, 5, 9 })
           ) == null &&
           bloqueos.Count == 1 &&
           bloqueos.First().id_bloqueador == 5 &&
           bloqueos.First().id_bloqueado == 9
       );
        }

            [Fact]
        public void RemoveBlock_BlockNotFound_ThrowsException()
        {
             
            var bloqueos = new List<bloqueos>();
            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemoveBlock", BindingFlags.NonPublic | BindingFlags.Static);


            Assert.True(
    method is not null &&
    Assert.Throws<TargetInvocationException>(() =>
        method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
    ).InnerException is RepositoryValidationException
);

        }

        [Fact]
        public void RemoveBlock_ExistingBlock_RemovesEntry()
        {

            var bloqueos = new List<bloqueos>
    {
        new bloqueos { id_bloqueador = 1, id_bloqueado = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemoveBlock", BindingFlags.NonPublic | BindingFlags.Static);


            Assert.True(
         method is not null &&
         Record.Exception(() =>
             method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
         ) == null &&
         !bloqueos.Any()
     );
        
        }

        [Fact]
        public void RemoveBlock_NoMatchingBlock_ThrowsException()
        {
             
            var bloqueos = new List<bloqueos>
    {
        new bloqueos { id_bloqueador = 99, id_bloqueado = 100 },
        new bloqueos { id_bloqueador = 5, id_bloqueado = 8 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemoveBlock", BindingFlags.NonPublic | BindingFlags.Static);


            Assert.True(
       method is not null &&
       Assert.Throws<TargetInvocationException>(() =>
           method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
       ).InnerException is RepositoryValidationException
   );

        }

        [Fact]
        public void RemoveBlock_MultipleMatchingBlocks_RemovesOnlyFirst()
        {
             
            var bloqueos = new List<bloqueos>
    {
        new bloqueos { id_bloqueador = 1, id_bloqueado = 2 },
        new bloqueos { id_bloqueador = 1, id_bloqueado = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemoveBlock", BindingFlags.NonPublic | BindingFlags.Static);


            Assert.True(
             method is not null &&
             Record.Exception(() =>
                 method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
             ) == null &&
             bloqueos.Count == 1
         );
        }

        [Fact]
        public void RemoveFriendshipIfExists_FriendshipFound_RemovesEntry()
        {
             
            var amistades = new List<amistades>
    {
        new amistades { id_usuario1 = 1, id_usuario2 = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemoveFriendshipIfExists", BindingFlags.NonPublic | BindingFlags.Static);


            Assert.True(
    method is not null &&
    Record.Exception(() =>
        method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
    ) == null &&
    !amistades.Any()
);

        }

        [Fact]
        public void RemoveFriendshipIfExists_FriendshipFound_ReversedIds_RemovesEntry()
        {
             
            var amistades = new List<amistades>
    {
        new amistades { id_usuario1 = 2, id_usuario2 = 1 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemoveFriendshipIfExists", BindingFlags.NonPublic | BindingFlags.Static);


            Assert.True(
    method is not null &&
    Record.Exception(() =>
        method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
    ) == null &&
    !amistades.Any()
);

        }

        [Fact]
        public void RemoveFriendshipIfExists_NoFriendship_NoChanges()
        {
             
            var amistades = new List<amistades>
    {
        new amistades { id_usuario1 = 10, id_usuario2 = 20 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemoveFriendshipIfExists", BindingFlags.NonPublic | BindingFlags.Static);


            Assert.True(
          method is not null &&
          Record.Exception(() =>
              method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
          ) == null &&
          amistades.Count == 1
      );

        }

        [Fact]
        public void RemoveFriendshipIfExists_MultipleFriendships_RemovesOnlyFirstMatch()
        {
             
            var amistades = new List<amistades>
    {
        new amistades { id_usuario1 = 1, id_usuario2 = 2 },
        new amistades { id_usuario1 = 1, id_usuario2 = 2 },
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemoveFriendshipIfExists", BindingFlags.NonPublic | BindingFlags.Static);


            Assert.True(
    method is not null &&
    Record.Exception(() =>
        method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
    ) == null &&
    amistades.Count == 1
);

        }

        [Fact]
        public void RemoveFriendshipIfExists_EmptyList_NoException()
        {
             
            var amistades = new List<amistades>();

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemoveFriendshipIfExists", BindingFlags.NonPublic | BindingFlags.Static);


            Assert.True(
    method is not null &&
    Record.Exception(() =>
        method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
    ) == null
);

        }

        [Fact]
        public void RemovePendingRequests_ExactMatch_RemovesRequest()
        {
             
            var requests = new List<solicitudes_amistad>
    {
        new solicitudes_amistad { id_emisor = 1, id_receptor = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(requests).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemovePendingRequests", BindingFlags.NonPublic | BindingFlags.Static);


            Assert.True(
    method is not null &&
    Record.Exception(() =>
        method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
    ) == null &&
    !requests.Any()
);

        }

        [Fact]
        public void RemovePendingRequests_ReversedMatch_RemovesRequest()
        {
             
            var requests = new List<solicitudes_amistad>
    {
        new solicitudes_amistad { id_emisor = 2, id_receptor = 1 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(requests).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemovePendingRequests", BindingFlags.NonPublic | BindingFlags.Static);

            Assert.True(
    method is not null &&
    Record.Exception(() =>
        method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
    ) == null &&
    !requests.Any()
);

        }

        [Fact]
        public void RemovePendingRequests_NoMatchingRequests_NoChanges()
        {
             
            var requests = new List<solicitudes_amistad>
    {
        new solicitudes_amistad { id_emisor = 5, id_receptor = 7 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(requests).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemovePendingRequests", BindingFlags.NonPublic | BindingFlags.Static);


            Assert.True(
        method is not null &&
        Record.Exception(() =>
            method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
        ) == null &&
        requests.Count == 1
    );

        }

        [Fact]
        public void RemovePendingRequests_MultipleMatches_RemovesAllMatches()
        {
             
            var requests = new List<solicitudes_amistad>
    {
        new solicitudes_amistad { id_emisor = 1, id_receptor = 2 },
        new solicitudes_amistad { id_emisor = 2, id_receptor = 1 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(requests).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemovePendingRequests", BindingFlags.NonPublic | BindingFlags.Static);


            Assert.True(
    method is not null &&
    Record.Exception(() =>
        method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
    ) == null &&
    !requests.Any()
);

        }

        [Fact]
        public void RemovePendingRequests_EmptyList_NoException()
        {
             
            var requests = new List<solicitudes_amistad>();

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(requests).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemovePendingRequests", BindingFlags.NonPublic | BindingFlags.Static);


            Assert.True(
    method is not null &&
    Record.Exception(() =>
        method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
    ) == null
);

        }

        [Fact]
        public void RemovePendingRequests_MixedRequests_RemovesOnlyThoseMatchingUsers()
        {
             
            var requests = new List<solicitudes_amistad>
    {
        new solicitudes_amistad { id_emisor = 1, id_receptor = 2 },
        new solicitudes_amistad { id_emisor = 3, id_receptor = 4 },
        new solicitudes_amistad { id_emisor = 2, id_receptor = 1 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(requests).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemovePendingRequests", BindingFlags.NonPublic | BindingFlags.Static);


            Assert.True(
       method is not null &&
       Record.Exception(() =>
           method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
       ) == null &&
       requests.Count == 1 &&
       requests[0].id_emisor == 3 &&
       requests[0].id_receptor == 4
   );

        }

        [Fact]
        public void GetFriends_NoFriendships_ReturnsEmptyList()
        {
             
            var friendships = new List<amistades>();

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(friendships).Object);

            var mockUserRepo = new Mock<IRepositoryUsers>();
            mockUserRepo.Setup(r => r.GetUserIdByUsername("seth")).Returns(1);

            var repo = new FriendRepository(mockUserRepo.Object, () => mockDb.Object);

             
            var result = repo.GetFriends("seth");

             
            Assert.Empty(result);
        }


        [Fact]
        public void GetFriends_OneFriendshipWhereUserIsUser1_ReturnsFriendDto()
        {
             
            var friendUser = new usuarios
            {
                id_usuario = 2,
                perfiles = new List<perfiles> { new perfiles { username = "amigo" } }
            };

            var friendships = new List<amistades>
    {
        new amistades
        {
            id_usuario1 = 1,
            id_usuario2 = 2,
            usuarios1 = friendUser
        }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(friendships).Object);

            var mockUserRepo = new Mock<IRepositoryUsers>();
            mockUserRepo.Setup(r => r.GetUserIdByUsername("seth")).Returns(1);

            var repo = new FriendRepository(mockUserRepo.Object, () => mockDb.Object);

             
            var result = repo.GetFriends("seth");

            Assert.True(
      result.Count == 1 &&
      result[0].IdFriend == 2 &&
      result[0].Username == "amigo"
  );

        }


        [Fact]
        public void GetFriends_OneFriendshipWhereUserIsUser2_ReturnsFriendDto()
        {
             
            var friendUser = new usuarios
            {
                id_usuario = 3,
                perfiles = new List<perfiles> { new perfiles { username = "otro" } }
            };

            var friendships = new List<amistades>
    {
        new amistades
        {
            id_usuario1 = 3,
            id_usuario2 = 1,
            usuarios = friendUser
        }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(friendships).Object);

            var mockUserRepo = new Mock<IRepositoryUsers>();
            mockUserRepo.Setup(r => r.GetUserIdByUsername("seth")).Returns(1);

            var repo = new FriendRepository(mockUserRepo.Object, () => mockDb.Object);

             
            var result = repo.GetFriends("seth");


            Assert.True(
    result.Count == 1 &&
    result[0].IdFriend == 3 &&
    result[0].Username == "otro"
);

        }

        [Fact]
        public void GetFriends_MultipleFriendships_ReturnsAllFriends()
        {
             
            var friend1 = new usuarios
            {
                id_usuario = 2,
                perfiles = new List<perfiles> { new perfiles { username = "uno" } }
            };

            var friend2 = new usuarios
            {
                id_usuario = 3,
                perfiles = new List<perfiles> { new perfiles { username = "dos" } }
            };

            var friendships = new List<amistades>
    {
        new amistades { id_usuario1 = 1, id_usuario2 = 2, usuarios1 = friend1 },
        new amistades { id_usuario1 = 3, id_usuario2 = 1, usuarios = friend2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(friendships).Object);

            var mockUserRepo = new Mock<IRepositoryUsers>();
            mockUserRepo.Setup(r => r.GetUserIdByUsername("seth")).Returns(1);

            var repo = new FriendRepository(mockUserRepo.Object, () => mockDb.Object);

             
            var result = repo.GetFriends("seth");

            Assert.True(
   result.Count == 2 &&
   result.Any(f => f.IdFriend == 2) &&
   result.Any(f => f.IdFriend == 3)
);

        }


        [Fact]
        public void GetFriends_FriendWithoutProfile_UsesDefaultValues()
        {
             
            var friendUser = new usuarios
            {
                id_usuario = 7,
                perfiles = new List<perfiles>() 
            };

            var friendships = new List<amistades>
    {
        new amistades
        {
            id_usuario1 = 1,
            id_usuario2 = 7,
            usuarios1 = friendUser
        }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(friendships).Object);

            var mockUserRepo = new Mock<IRepositoryUsers>();
            mockUserRepo.Setup(r => r.GetUserIdByUsername("seth")).Returns(1);

            var repo = new FriendRepository(mockUserRepo.Object, () => mockDb.Object);

             
            var result = repo.GetFriends("seth");

            Assert.True(
                result.Count == 1 &&
                result[0].IdFriend == 7 &&
                result[0].Username == "N/A"
            );

        }


        [Fact]
        public void GetFriendRequests_NoRequests_ReturnsEmptyList()
        {
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("seth")).Returns(1);

            var solicitudes = new List<solicitudes_amistad>();

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

            var result = repo.GetFriendRequests("seth");

            Assert.Empty(result);
        }
        [Fact]
        public void GetFriendRequests_OnePendingRequest_ReturnsFriendDto()
        {
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("seth")).Returns(1);

            var emitter = new usuarios
            {
                id_usuario = 2,
                perfiles = new List<perfiles> { new perfiles { username = "amigo" } }
            };

            var solicitudes = new List<solicitudes_amistad>
    {
        new solicitudes_amistad
        {
            id_emisor = 2,
            id_receptor = 1,
            estado = "pendiente",
            usuarios = emitter
        }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

            var result = repo.GetFriendRequests("seth");

            Assert.True(
                result.Count == 1 &&
                result[0].IdFriend == 2 &&
                result[0].Username == "amigo"
            );

        }


        [Fact]
        public void GetFriendRequests_IgnoresNonPendingRequests()
        {
             
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("seth")).Returns(1);

            var solicitudes = new List<solicitudes_amistad>
    {
        new solicitudes_amistad { id_emisor = 2, id_receptor = 1, estado = "rechazada" },
        new solicitudes_amistad { id_emisor = 3, id_receptor = 1, estado = "aceptada" }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

             
            var result = repo.GetFriendRequests("seth");

             
            Assert.Empty(result);
        }


        [Fact]
        public void GetFriendRequests_IgnoresRequestsForOtherUsers()
        {
             
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("seth")).Returns(10);

            var solicitudes = new List<solicitudes_amistad>
    {
        new solicitudes_amistad { id_emisor = 2, id_receptor = 99, estado = "pendiente" }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

             
            var result = repo.GetFriendRequests("seth");

             
            Assert.Empty(result);
        }

        [Fact]
        public void GetFriendRequests_EmitterHasNoProfile_UsesDefaultValues()
        {
             
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("seth")).Returns(1);

            var user = new usuarios
            {
                id_usuario = 5,
                perfiles = new List<perfiles>() 
            };

            var solicitudes = new List<solicitudes_amistad>
    {
        new solicitudes_amistad
        {
            id_emisor = 5,
            id_receptor = 1,
            estado = "pendiente",
            usuarios = user
        }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

             
            var result = repo.GetFriendRequests("seth");

            Assert.True(
   result.Count == 1 &&
   result[0].Username == "N/A"
);

        }

        [Fact]
        public void UpdateBlockStatus_BlockSelf_ThrowsUserValidationError()
        {
             
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("same")).Returns(10);

            var mockDb = new Mock<damas_chinasEntities>();

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

               
            Assert.Throws<RepositoryValidationException>(() =>
                repo.UpdateBlockStatus("same", "same", true)
            );
        }

        [Fact]
        public void UpdateBlockStatus_Block_AddsBlockEntry()
        {
             
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("A")).Returns(1);
            mockRepo.Setup(r => r.GetUserIdByUsername("B")).Returns(2);

            var bloqueos = new List<bloqueos>();
            var amistades = new List<amistades>();          
            var solicitudes = new List<solicitudes_amistad>(); 

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

             
            repo.UpdateBlockStatus("A", "B", true);

             
            Assert.Single(bloqueos); 
        }



        [Fact]
        public void UpdateBlockStatus_BlockAlreadyBlocked_ThrowsException()
        {
             
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("A")).Returns(1);
            mockRepo.Setup(r => r.GetUserIdByUsername("B")).Returns(2);

            var bloqueos = new List<bloqueos>
    {
        new bloqueos { id_bloqueador = 1, id_bloqueado = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();

            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<amistades>()).Object);

            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<solicitudes_amistad>()).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

               
            Assert.Throws<RepositoryValidationException>(() =>
                repo.UpdateBlockStatus("A", "B", true)
            );
        }

        [Fact]
        public void UpdateBlockStatus_Unblock_RemovesEntry()
        {
             
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("A")).Returns(1);
            mockRepo.Setup(r => r.GetUserIdByUsername("B")).Returns(2);

            var bloqueos = new List<bloqueos>
    {
        new bloqueos { id_bloqueador = 1, id_bloqueado = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();

            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<amistades>()).Object);

            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<solicitudes_amistad>()).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

             
            repo.UpdateBlockStatus("A", "B", false);

             
            Assert.Empty(bloqueos);
        }




        [Fact]
        public void UpdateBlockStatus_UnblockMissing_ThrowsException()
        {
             
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("A")).Returns(1);
            mockRepo.Setup(r => r.GetUserIdByUsername("B")).Returns(2);

            var bloqueos = new List<bloqueos>();

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

               
            Assert.Throws<RepositoryValidationException>(() =>
                repo.UpdateBlockStatus("A", "B", false)
            );
        }

        [Fact]
        public void DeleteFriend_ValidFriendship_RemovesFriendAndReturnsTrue()
        {
             
            var mockUserRepo = new Mock<IRepositoryUsers>();
            mockUserRepo.Setup(r => r.GetUserIdByUsername("A")).Returns(1);
            mockUserRepo.Setup(r => r.GetUserIdByUsername("B")).Returns(2);

            var friendships = new List<amistades>
    {
        new amistades { id_usuario1 = 1, id_usuario2 = 2 }
    };

            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1 },
        new usuarios { id_usuario = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(friendships).Object);

            var repo = new FriendRepository(mockUserRepo.Object, () => mockDb.Object);

             
            var result = repo.DeleteFriend("A", "B");

            Assert.True(
    result &&
    !friendships.Any()
);

        }

        [Fact]
        public void DeleteFriend_NoFriendship_ThrowsFriendsLoadError()
        {
             
            var mockUserRepo = new Mock<IRepositoryUsers>();
            mockUserRepo.Setup(r => r.GetUserIdByUsername("A")).Returns(1);
            mockUserRepo.Setup(r => r.GetUserIdByUsername("B")).Returns(2);

            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1 },
        new usuarios { id_usuario = 2 }
    };

            var friendships = new List<amistades>(); 

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(friendships).Object);

            var repo = new FriendRepository(mockUserRepo.Object, () => mockDb.Object);

               
            Assert.Throws<RepositoryValidationException>(() =>
                repo.DeleteFriend("A", "B")
            );
        }

        [Fact]
        public void DeleteFriend_UserDoesNotExist_ThrowsException()
        {
             
            var mockUserRepo = new Mock<IRepositoryUsers>();
            mockUserRepo.Setup(r => r.GetUserIdByUsername("A")).Returns(1);
            mockUserRepo.Setup(r => r.GetUserIdByUsername("B")).Returns(999);

            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1 },
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new FriendRepository(mockUserRepo.Object, () => mockDb.Object);

               
            Assert.Throws<RepositoryValidationException>(() =>
                repo.DeleteFriend("A", "B")
            );
        }

        [Fact]
        public void DeleteFriend_FriendshipReverseOrder_RemovesCorrectEntry()
        {
             
            var mockUserRepo = new Mock<IRepositoryUsers>();
            mockUserRepo.Setup(r => r.GetUserIdByUsername("A")).Returns(1);
            mockUserRepo.Setup(r => r.GetUserIdByUsername("B")).Returns(2);

            var friendships = new List<amistades>
    {
        new amistades { id_usuario1 = 2, id_usuario2 = 1 }
    };

            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1 },
        new usuarios { id_usuario = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(friendships).Object);

            var repo = new FriendRepository(mockUserRepo.Object, () => mockDb.Object);

             
            repo.DeleteFriend("A", "B");

             
            Assert.Empty(friendships);
        }

        [Fact]
        public void DeleteFriend_SaveChanges_IsCalled()
        {
            var mockUserRepo = new Mock<IRepositoryUsers>();
            mockUserRepo.Setup(r => r.GetUserIdByUsername("A")).Returns(1);
            mockUserRepo.Setup(r => r.GetUserIdByUsername("B")).Returns(2);

            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1 },
        new usuarios { id_usuario = 2 }
    };

            var friendships = new List<amistades>
    {
        new amistades { id_usuario1 = 1, id_usuario2 = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(friendships).Object);

            mockDb.Setup(db => db.SaveChanges()).Verifiable();

            var repo = new FriendRepository(mockUserRepo.Object, () => mockDb.Object);


            Assert.True(
    Record.Exception(() => repo.DeleteFriend("A", "B")) == null &&
    mockDb.Invocations.Count(i => i.Method.Name == nameof(damas_chinasEntities.SaveChanges)) == 1
);

        }

        [Fact]
        public void GetFriendPublicProfile_ValidUserWithProfile_ReturnsCorrectData()
        {
             
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("seth")).Returns(1);

            var perfiles = new List<perfiles>
    {
        new perfiles
        {
            username = "seth",
            nombre = "Seth",
            apellido_materno = "Marquez",
            url = "http://test.com",
            imagen_perfil = "img.png"
        }
    };

            var user = new usuarios
            {
                id_usuario = 1,
                perfiles = perfiles
            };

            var stats = new List<participantes_partida>
    {
        new participantes_partida { id_jugador = 1, posicion_final = 1 },
        new participantes_partida { id_jugador = 1, posicion_final = 3 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<usuarios> { user }).Object);

            mockDb.Setup(db => db.participantes_partida)
                  .Returns(MockDbSetHelper.CreateMockSet(stats).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

             
            var result = repo.GetFriendPublicProfile("seth");

             
           
        }

        [Fact]
        public void GetFriendPublicProfile_UserWithoutProfile_UsesDefaultValues()
        {
             
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("user")).Returns(1);

            var user = new usuarios
            {
                id_usuario = 1,
                perfiles = new List<perfiles>() 
            };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<usuarios> { user }).Object);

            mockDb.Setup(db => db.participantes_partida)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<participantes_partida>()).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

             
            var result = repo.GetFriendPublicProfile("user");

            Assert.True(
     result.Username == "" &&
     result.Name == "" &&
     result.LastName == "" &&
     result.SocialUrl == "" &&
     result.AvatarFile == "avatarIcon.png"
 );

        }

        [Fact]
        public void GetFriendPublicProfile_UserNotFound_ThrowsException()
        {
             
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("ghost")).Returns(99);

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<usuarios>()).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

               
            Assert.Throws<RepositoryValidationException>(() =>
                repo.GetFriendPublicProfile("ghost")
            );
        }

        [Fact]
        public void GetFriendPublicProfile_NoMatches_ShouldReturnZeroStats()
        {
             
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("player")).Returns(1);

            var user = new usuarios
            {
                id_usuario = 1,
                perfiles = new List<perfiles>
        {
            new perfiles { username = "player" }
        }
            };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<usuarios> { user }).Object);

            mockDb.Setup(db => db.participantes_partida)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<participantes_partida>()).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

             
            var result = repo.GetFriendPublicProfile("player");


            Assert.True(
     result.MatchesPlayed == 0 &&
     result.Wins == 0 &&
     result.Loses == 0
 );

        }

        [Fact]
        public void GetFriendPublicProfile_NullAvatar_UsesDefaultAvatar()
        {
             
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("pic")).Returns(1);

            var user = new usuarios
            {
                id_usuario = 1,
                perfiles = new List<perfiles>
        {
            new perfiles { username = "pic", imagen_perfil = null }
        }
            };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<usuarios> { user }).Object);

            mockDb.Setup(db => db.participantes_partida)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<participantes_partida>()).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

             
            var result = repo.GetFriendPublicProfile("pic");

             
            Assert.Equal("avatarIcon.png", result.AvatarFile);
        }


        [Fact]
        public void DeleteFriendAndBlock_AlreadyBlocked_ThrowsException()
        {
             
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("A")).Returns(1);
            mockRepo.Setup(r => r.GetUserIdByUsername("B")).Returns(2);

            var amistades = new List<amistades>();
            var bloqueos = new List<bloqueos>
    {
        new bloqueos { id_bloqueador = 1, id_bloqueado = 2 }
    };

            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1 },
        new usuarios { id_usuario = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios).Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);
            mockDb.Setup(db => db.amistades).Returns(MockDbSetHelper.CreateMockSet(amistades).Object);
            mockDb.Setup(db => db.bloqueos).Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

               
            Assert.Throws<RepositoryValidationException>(() =>
                repo.DeleteFriendAndBlock("A", "B")
            );
        }

        [Fact]
        public void DeleteFriendAndBlock_SameUser_ThrowsException()
        {
             
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("A")).Returns(1);

            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios).Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

               
            Assert.Throws<RepositoryValidationException>(() =>
                repo.DeleteFriendAndBlock("A", "A")
            );
        }

        [Fact]
        public void DeleteFriendAndBlock_UserNotFound_ThrowsException()
        {
             
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("A")).Returns(1);
            mockRepo.Setup(r => r.GetUserIdByUsername("B")).Returns(2);

            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1 }
     
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios).Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

               
            Assert.Throws<RepositoryValidationException>(() =>
                repo.DeleteFriendAndBlock("A", "B")
            );
        }

    }
}
