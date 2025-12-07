using DamasChinas_Server;
using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
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
            // Arrange
            var method = typeof(FriendRepository)
                .GetMethod("EnsureDifferentUsers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            int sameId = 5;

            // Act
            var ex = Record.Exception(() => method.Invoke(null, new object[] { sameId, sameId }));

            // Assert
            Assert.IsType<RepositoryValidationException>(ex.InnerException);
            Assert.Equal(MessageCode.UserValidationError, ((RepositoryValidationException)ex.InnerException).Code);
        }

        [Fact]
        public void EnsureDifferentUsers_DifferentIds_DoesNotThrow()
        {
            // Arrange
            var method = typeof(FriendRepository)
                .GetMethod("EnsureDifferentUsers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act
            var ex = Record.Exception(() => method.Invoke(null, new object[] { 1, 2 }));

            // Assert
            Assert.Null(ex);
        }

        [Fact]
        public void EnsureUsersExist_UserMissing_ThrowsUserNotFound()
        {
            // Arrange
            var method = typeof(FriendRepository)
                .GetMethod("EnsureUsersExist", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsureUsersExist no encontrado.");

            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios).Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            // Act
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object!, 1, 99 })
            );

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<RepositoryValidationException>(exception.InnerException);
            Assert.Equal(
                MessageCode.UserNotFound,
                ((RepositoryValidationException)exception.InnerException!).Code
            );
        }


        [Fact]
        public void EnsureUsersExist_BothExist_DoesNotThrow()
        {
            // Arrange
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

            // Act
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object!, 1, 2 })
            );

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void EnsureNotFriends_AlreadyFriends_ThrowsFriendsLoadError()
        {
            // Arrange
            var method = typeof(FriendRepository)
                .GetMethod("EnsureNotFriends", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsureNotFriends no encontrado.");

            var amistades = new List<amistades>
    {
        new amistades { id_usuario1 = 1, id_usuario2 = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades).Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

            // Act
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object!, 1, 2 })
            );

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<RepositoryValidationException>(exception.InnerException);
            Assert.Equal(
                MessageCode.FriendsLoadError,
                ((RepositoryValidationException)exception.InnerException!).Code
            );
        }

        [Fact]
        public void EnsureNotFriends_NoFriendship_DoesNotThrow()
        {
            // Arrange
            var method = typeof(FriendRepository)
                .GetMethod("EnsureNotFriends", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsureNotFriends no encontrado.");

            var amistades = new List<amistades>(); // vacío = no son amigos

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades).Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

            // Act
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object!, 1, 2 })
            );

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void EnsureFriends_NoFriendship_ThrowsFriendsLoadError()
        {
            // Arrange
            var method = typeof(FriendRepository)
                .GetMethod("EnsureFriends", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsureFriends no encontrado.");

            var amistades = new List<amistades>(); // no existe amistad

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades)
                .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

            // Act
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object!, 1, 2 })
            );

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<RepositoryValidationException>(exception.InnerException);
            Assert.Equal(
                MessageCode.FriendsLoadError,
                ((RepositoryValidationException)exception.InnerException!).Code
            );
        }

        [Fact]
        public void EnsureFriends_FriendshipExists_DoesNotThrow()
        {
            // Arrange
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

            // Act
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object!, 1, 2 })
            );

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void EnsureNotBlocked_IsBlocked_ThrowsFriendsLoadError()
        {
            // Arrange
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

            // Act
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object!, 1, 2 })
            );

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<RepositoryValidationException>(exception.InnerException);
            Assert.Equal(
                MessageCode.FriendsLoadError,
                ((RepositoryValidationException)exception.InnerException!).Code
            );
        }

        [Fact]
        public void EnsureNotBlocked_NoBlock_DoesNotThrow()
        {
            // Arrange
            var method = typeof(FriendRepository)
                .GetMethod("EnsureNotBlocked", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsureNotBlocked no encontrado.");

            var bloqueos = new List<bloqueos>(); // vacío → nadie está bloqueado

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.bloqueos)
                .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            // Act
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object!, 1, 2 })
            );

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void EnsureNotBlockedSelf_SameUser_ThrowsUserValidationError()
        {
            // Arrange
            var method = typeof(FriendRepository)
                .GetMethod("EnsureNotBlockedSelf", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsureNotBlockedSelf no encontrado.");

            // Act
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { 5, 5 }) // mismo ID
            );

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<RepositoryValidationException>(exception.InnerException);
            Assert.Equal(
                MessageCode.UserValidationError,
                ((RepositoryValidationException)exception.InnerException!).Code
            );
        }

        [Fact]
        public void EnsureNotBlockedSelf_DifferentUsers_DoesNotThrow()
        {
            // Arrange
            var method = typeof(FriendRepository)
                .GetMethod("EnsureNotBlockedSelf", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsureNotBlockedSelf no encontrado.");

            // Act
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { 1, 2 })
            );

            // Assert
            Assert.Null(exception);
        }
        [Fact]
        public void EnsureNoPendingRequest_PendingRequestExists_ThrowsFriendsLoadError()
        {
            // Arrange
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

            // Act
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
            );

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<RepositoryValidationException>(exception.InnerException);
            Assert.Equal(
                MessageCode.FriendsLoadError,
                ((RepositoryValidationException)exception.InnerException!).Code
            );
        }

        [Fact]
        public void EnsureNoPendingRequest_NoPendingRequest_DoesNotThrow()
        {
            // Arrange
            var solicitudes = new List<solicitudes_amistad>(); // vacío

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            var method = typeof(FriendRepository)
                .GetMethod("EnsureNoPendingRequest", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsureNoPendingRequest no encontrado.");

            // Act
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
            );

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void EnsurePendingRequestExists_NoPending_ThrowsFriendsLoadError()
        {
            // Arrange
            var solicitudes = new List<solicitudes_amistad>(); // no hay solicitudes

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            var method = typeof(FriendRepository)
                .GetMethod("EnsurePendingRequestExists", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsurePendingRequestExists no encontrado.");

            // Act
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
            );

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<RepositoryValidationException>(exception.InnerException);
            Assert.Equal(
                MessageCode.FriendsLoadError,
                ((RepositoryValidationException)exception.InnerException!).Code
            );
        }

        [Fact]
        public void EnsurePendingRequestExists_PendingExists_DoesNotThrow()
        {
            // Arrange
            var solicitudes = new List<solicitudes_amistad>
    {
        new solicitudes_amistad
        {
            id_emisor = 2,   // receptor → emisor
            id_receptor = 1,
            estado = "pendiente"
        }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            var method = typeof(FriendRepository)
                .GetMethod("EnsurePendingRequestExists", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método EnsurePendingRequestExists no encontrado.");

            // Act
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
            );

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void FriendshipExists_WhenFriendshipExists_ReturnsTrue()
        {
            // Arrange
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

            // Act
            var result = (bool)method.Invoke(null, new object[] { mockDb.Object, 1, 2 })!;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void FriendshipExists_WhenFriendshipDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var amistades = new List<amistades>(); // vacío

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

            var method = typeof(FriendRepository)
                .GetMethod("FriendshipExists", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método FriendshipExists no encontrado.");

            // Act
            var result = (bool)method.Invoke(null, new object[] { mockDb.Object, 1, 2 })!;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsBlocked_WhenUserIsBlocked_ReturnsTrue()
        {
            // Arrange
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

            // Act
            var result = (bool)method.Invoke(null, new object[] { mockDb.Object, 1, 2 })!;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsBlocked_WhenUserIsNotBlocked_ReturnsFalse()
        {
            // Arrange
            var bloqueos = new List<bloqueos>(); // vacío

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            var method = typeof(FriendRepository)
                .GetMethod("IsBlocked", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método IsBlocked no encontrado.");

            // Act
            var result = (bool)method.Invoke(null, new object[] { mockDb.Object, 1, 2 })!;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void PendingRequestExists_WhenPendingRequestExists_ReturnsTrue()
        {
            // Arrange
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

            // Act
            var result = (bool)method.Invoke(null, new object[] { mockDb.Object, 1, 2 })!;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void PendingRequestExists_WhenNoPendingRequest_ReturnsFalse()
        {
            // Arrange
            var solicitudes = new List<solicitudes_amistad>(); // vacío → no hay solicitudes

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            var method = typeof(FriendRepository)
                .GetMethod("PendingRequestExists", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new Exception("Método PendingRequestExists no encontrado.");

            // Act
            var result = (bool)method.Invoke(null, new object[] { mockDb.Object, 1, 2 })!;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void MapToFriendDto_WithValidProfile_ReturnsCorrectDto()
        {
            // Arrange
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
                .GetMethod("MapToFriendDto", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act
            var dto = (FriendDto)method.Invoke(null, new object[] { user });

            // Assert
            Assert.Equal(10, dto.IdFriend);
            Assert.Equal("Seth", dto.Username);
            Assert.False(dto.ConnectionState);
            Assert.Equal("avatar123.png", dto.Avatar);
        }

        [Fact]
        public void MapToFriendDto_NoProfile_ReturnsDefaultValues()
        {
            // Arrange
            var user = new usuarios
            {
                id_usuario = 55,
                perfiles = new List<perfiles>() // vacío
            };

            var method = typeof(FriendRepository)
                .GetMethod("MapToFriendDto", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act
            var dto = (FriendDto)method.Invoke(null, new object[] { user });

            // Assert
            Assert.Equal(55, dto.IdFriend);
            Assert.Equal("N/A", dto.Username);
            Assert.False(dto.ConnectionState);
            Assert.Equal("default.png", dto.Avatar);
        }

        [Fact]
        public void GetUserIds_ValidDifferentUsers_ReturnsCorrectTuple()
        {
            // Arrange
            var mockUserRepo = new Mock<IRepositoryUsers>();

            mockUserRepo.Setup(r => r.GetUserIdByUsername("Receiver"))
                        .Returns(10);

            mockUserRepo.Setup(r => r.GetUserIdByUsername("Sender"))
                        .Returns(20);

            var repo = new FriendRepository(mockUserRepo.Object, () => null);

            var method = typeof(FriendRepository)
                .GetMethod("GetUserIds",
                    BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var result = ((int receiverId, int senderId))method.Invoke(
                repo, new object[] { "Receiver", "Sender" }
            );

            // Assert
            Assert.Equal(10, result.receiverId);
            Assert.Equal(20, result.senderId);
        }


        [Fact]
        public void GetUserIds_SameUser_ThrowsUserValidationError()
        {
            // Arrange
            var mockUserRepo = new Mock<IRepositoryUsers>();

            mockUserRepo.Setup(r => r.GetUserIdByUsername(It.IsAny<string>()))
                        .Returns(50); 
            var repo = new FriendRepository(mockUserRepo.Object, () => null);

            var method = typeof(FriendRepository)
                .GetMethod(
                    "GetUserIds",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

            // Act
            var ex = Assert.Throws<TargetInvocationException>(() =>
                method.Invoke(repo, new object[] { "User", "User" })
            );

            // Assert
            Assert.IsType<RepositoryValidationException>(ex.InnerException);
        }

        [Fact]
        public void ApplyBlock_AlreadyBlocked_ThrowsException()
        {
            // Arrange
            var bloqueos = new List<bloqueos>
    {
        new bloqueos { id_bloqueador = 1, id_bloqueado = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            var method = typeof(FriendRepository)
                .GetMethod("ApplyBlock", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            var ex = Assert.Throws<TargetInvocationException>(() =>
                method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
            );

            // Assert
            Assert.IsType<RepositoryValidationException>(ex.InnerException);
         
        }

        [Fact]
        public void ApplyBlock_RemovesFriendshipAndPendingRequests()
        {
            // Arrange
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

            // Act
            method.Invoke(null, new object[] { mockDb.Object, 1, 2 });

            // Assert
            Assert.Empty(amistades);
            Assert.Empty(solicitudes);
        }

        [Fact]
        public void ApplyBlock_AddsNewBlockEntry()
        {
            // Arrange
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

            // Act
            method.Invoke(null, new object[] { mockDb.Object, 5, 9 });

            // Assert
            Assert.Single(bloqueos);

            var block = bloqueos.First();
            Assert.Equal(5, block.id_bloqueador);
            Assert.Equal(9, block.id_bloqueado);
        }

        [Fact]
        public void RemoveBlock_BlockNotFound_ThrowsException()
        {
            // Arrange
            var bloqueos = new List<bloqueos>();
            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemoveBlock", BindingFlags.NonPublic | BindingFlags.Static);

            // Act & Assert
            var ex = Assert.Throws<TargetInvocationException>(() =>
                method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
            );

            Assert.IsType<RepositoryValidationException>(ex.InnerException);
        }

        [Fact]
        public void RemoveBlock_ExistingBlock_RemovesEntry()
        {
            // Arrange
            var bloqueos = new List<bloqueos>
    {
        new bloqueos { id_bloqueador = 1, id_bloqueado = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemoveBlock", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            method.Invoke(null, new object[] { mockDb.Object, 1, 2 });

            // Assert
            Assert.Empty(bloqueos);
        }

        [Fact]
        public void RemoveBlock_NoMatchingBlock_ThrowsException()
        {
            // Arrange
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

            // Act & Assert
            var ex = Assert.Throws<TargetInvocationException>(() =>
                method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
            );

            Assert.IsType<RepositoryValidationException>(ex.InnerException);
        }

        [Fact]
        public void RemoveBlock_MultipleMatchingBlocks_RemovesOnlyFirst()
        {
            // Arrange
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

            // Act
            method.Invoke(null, new object[] { mockDb.Object, 1, 2 });

            // Assert
            Assert.Single(bloqueos);
        }

        [Fact]
        public void RemoveFriendshipIfExists_FriendshipFound_RemovesEntry()
        {
            // Arrange
            var amistades = new List<amistades>
    {
        new amistades { id_usuario1 = 1, id_usuario2 = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemoveFriendshipIfExists", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            method.Invoke(null, new object[] { mockDb.Object, 1, 2 });

            // Assert
            Assert.Empty(amistades);
        }

        [Fact]
        public void RemoveFriendshipIfExists_FriendshipFound_ReversedIds_RemovesEntry()
        {
            // Arrange
            var amistades = new List<amistades>
    {
        new amistades { id_usuario1 = 2, id_usuario2 = 1 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemoveFriendshipIfExists", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            method.Invoke(null, new object[] { mockDb.Object, 1, 2 });

            // Assert
            Assert.Empty(amistades);
        }

        [Fact]
        public void RemoveFriendshipIfExists_NoFriendship_NoChanges()
        {
            // Arrange
            var amistades = new List<amistades>
    {
        new amistades { id_usuario1 = 10, id_usuario2 = 20 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemoveFriendshipIfExists", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            method.Invoke(null, new object[] { mockDb.Object, 1, 2 });

            // Assert
            Assert.Single(amistades);
        }

        [Fact]
        public void RemoveFriendshipIfExists_MultipleFriendships_RemovesOnlyFirstMatch()
        {
            // Arrange
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

            // Act
            method.Invoke(null, new object[] { mockDb.Object, 1, 2 });

            // Assert
            Assert.Single(amistades);
        }

        [Fact]
        public void RemoveFriendshipIfExists_EmptyList_NoException()
        {
            // Arrange
            var amistades = new List<amistades>();

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemoveFriendshipIfExists", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
            );

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void RemovePendingRequests_ExactMatch_RemovesRequest()
        {
            // Arrange
            var requests = new List<solicitudes_amistad>
    {
        new solicitudes_amistad { id_emisor = 1, id_receptor = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(requests).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemovePendingRequests", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            method.Invoke(null, new object[] { mockDb.Object, 1, 2 });

            // Assert
            Assert.Empty(requests);
        }

        [Fact]
        public void RemovePendingRequests_ReversedMatch_RemovesRequest()
        {
            // Arrange
            var requests = new List<solicitudes_amistad>
    {
        new solicitudes_amistad { id_emisor = 2, id_receptor = 1 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(requests).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemovePendingRequests", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            method.Invoke(null, new object[] { mockDb.Object, 1, 2 });

            // Assert
            Assert.Empty(requests);
        }

        [Fact]
        public void RemovePendingRequests_NoMatchingRequests_NoChanges()
        {
            // Arrange
            var requests = new List<solicitudes_amistad>
    {
        new solicitudes_amistad { id_emisor = 5, id_receptor = 7 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(requests).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemovePendingRequests", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            method.Invoke(null, new object[] { mockDb.Object, 1, 2 });

            // Assert
            Assert.Single(requests);
        }

        [Fact]
        public void RemovePendingRequests_MultipleMatches_RemovesAllMatches()
        {
            // Arrange
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

            // Act
            method.Invoke(null, new object[] { mockDb.Object, 1, 2 });

            // Assert
            Assert.Empty(requests);
        }

        [Fact]
        public void RemovePendingRequests_EmptyList_NoException()
        {
            // Arrange
            var requests = new List<solicitudes_amistad>();

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(requests).Object);

            var method = typeof(FriendRepository)
                .GetMethod("RemovePendingRequests", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            var exception = Record.Exception(() =>
                method.Invoke(null, new object[] { mockDb.Object, 1, 2 })
            );

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void RemovePendingRequests_MixedRequests_RemovesOnlyThoseMatchingUsers()
        {
            // Arrange
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

            // Act
            method.Invoke(null, new object[] { mockDb.Object, 1, 2 });

            // Assert
            Assert.Single(requests);
            Assert.Equal(3, requests[0].id_emisor);
            Assert.Equal(4, requests[0].id_receptor);
        }

        [Fact]
        public void GetFriends_NoFriendships_ReturnsEmptyList()
        {
            // Arrange
            var friendships = new List<amistades>();

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(friendships).Object);

            var mockUserRepo = new Mock<IRepositoryUsers>();
            mockUserRepo.Setup(r => r.GetUserIdByUsername("seth")).Returns(1);

            var repo = new FriendRepository(mockUserRepo.Object, () => mockDb.Object);

            // Act
            var result = repo.GetFriends("seth");

            // Assert
            Assert.Empty(result);
        }


        [Fact]
        public void GetFriends_OneFriendshipWhereUserIsUser1_ReturnsFriendDto()
        {
            // Arrange
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

            // Act
            var result = repo.GetFriends("seth");

            // Assert
            Assert.Single(result);
            Assert.Equal(2, result[0].IdFriend);
            Assert.Equal("amigo", result[0].Username);
        }


        [Fact]
        public void GetFriends_OneFriendshipWhereUserIsUser2_ReturnsFriendDto()
        {
            // Arrange
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

            // Act
            var result = repo.GetFriends("seth");

            // Assert
            Assert.Single(result);
            Assert.Equal(3, result[0].IdFriend);
            Assert.Equal("otro", result[0].Username);
        }

        [Fact]
        public void GetFriends_MultipleFriendships_ReturnsAllFriends()
        {
            // Arrange
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

            // Act
            var result = repo.GetFriends("seth");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, f => f.IdFriend == 2);
            Assert.Contains(result, f => f.IdFriend == 3);
        }


        [Fact]
        public void GetFriends_FriendWithoutProfile_UsesDefaultValues()
        {
            // Arrange
            var friendUser = new usuarios
            {
                id_usuario = 7,
                perfiles = new List<perfiles>() // NO PROFILE
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

            // Act
            var result = repo.GetFriends("seth");

            // Assert
            Assert.Single(result);
            Assert.Equal(7, result[0].IdFriend);
            Assert.Equal("N/A", result[0].Username);
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

            Assert.Single(result);
            Assert.Equal(2, result[0].IdFriend);
            Assert.Equal("amigo", result[0].Username);
        }


        [Fact]
        public void GetFriendRequests_IgnoresNonPendingRequests()
        {
            // Arrange
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

            // Act
            var result = repo.GetFriendRequests("seth");

            // Assert
            Assert.Empty(result);
        }


        [Fact]
        public void GetFriendRequests_IgnoresRequestsForOtherUsers()
        {
            // Arrange
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

            // Act
            var result = repo.GetFriendRequests("seth");

            // Assert
            Assert.Empty(result);
        }


        [Fact]
        public void GetFriendRequests_MultiplePendingRequests_ReturnsAll()
        {
            // Arrange
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("seth")).Returns(1);

            var user1 = new usuarios
            {
                id_usuario = 2,
                perfiles = new List<perfiles> { new perfiles { username = "uno" } }
            };

            var user2 = new usuarios
            {
                id_usuario = 3,
                perfiles = new List<perfiles> { new perfiles { username = "dos" } }
            };

            var solicitudes = new List<solicitudes_amistad>
    {
        new solicitudes_amistad { id_emisor = 2, id_receptor = 1, estado = "pendiente", usuarios = user1 },
        new solicitudes_amistad { id_emisor = 3, id_receptor = 1, estado = "pendiente", usuarios = user2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

            // Act
            var result = repo.GetFriendRequests("seth");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.IdFriend == 2);
            Assert.Contains(result, r => r.IdFriend == 3);
        }


        [Fact]
        public void GetFriendRequests_EmitterHasNoProfile_UsesDefaultValues()
        {
            // Arrange
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("seth")).Returns(1);

            var user = new usuarios
            {
                id_usuario = 5,
                perfiles = new List<perfiles>() // SIN PERFIL
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

            // Act
            var result = repo.GetFriendRequests("seth");

            // Assert
            Assert.Single(result);
            Assert.Equal("N/A", result[0].Username);
        }

        [Fact]
        public void UpdateBlockStatus_BlockSelf_ThrowsUserValidationError()
        {
            // Arrange
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("same")).Returns(10);

            var mockDb = new Mock<damas_chinasEntities>();

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

            // Act & Assert
            Assert.Throws<RepositoryValidationException>(() =>
                repo.UpdateBlockStatus("same", "same", true)
            );
        }

        [Fact]
        public void UpdateBlockStatus_Block_AddsBlockEntry()
        {
            // Arrange
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("A")).Returns(1);
            mockRepo.Setup(r => r.GetUserIdByUsername("B")).Returns(2);

            var bloqueos = new List<bloqueos>();
            var amistades = new List<amistades>();          // ← necesario
            var solicitudes = new List<solicitudes_amistad>(); // ← necesario

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

            // Act
            repo.UpdateBlockStatus("A", "B", true);

            // Assert
            Assert.Single(bloqueos); // ahora SÍ queda en 1
        }



        [Fact]
        public void UpdateBlockStatus_BlockAlreadyBlocked_ThrowsException()
        {
            // Arrange
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

            // Act & Assert
            Assert.Throws<RepositoryValidationException>(() =>
                repo.UpdateBlockStatus("A", "B", true)
            );
        }

        [Fact]
        public void UpdateBlockStatus_Unblock_RemovesEntry()
        {
            // Arrange
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

            // Act
            repo.UpdateBlockStatus("A", "B", false);

            // Assert
            Assert.Empty(bloqueos);
        }




        [Fact]
        public void UpdateBlockStatus_UnblockMissing_ThrowsException()
        {
            // Arrange
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("A")).Returns(1);
            mockRepo.Setup(r => r.GetUserIdByUsername("B")).Returns(2);

            var bloqueos = new List<bloqueos>(); // no entry

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

            // Act & Assert
            Assert.Throws<RepositoryValidationException>(() =>
                repo.UpdateBlockStatus("A", "B", false)
            );
        }

        [Fact]
        public void DeleteFriend_ValidFriendship_RemovesFriendAndReturnsTrue()
        {
            // Arrange
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

            // Act
            var result = repo.DeleteFriend("A", "B");

            // Assert
            Assert.True(result);
            Assert.Empty(friendships);
        }

        [Fact]
        public void DeleteFriend_NoFriendship_ThrowsFriendsLoadError()
        {
            // Arrange
            var mockUserRepo = new Mock<IRepositoryUsers>();
            mockUserRepo.Setup(r => r.GetUserIdByUsername("A")).Returns(1);
            mockUserRepo.Setup(r => r.GetUserIdByUsername("B")).Returns(2);

            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1 },
        new usuarios { id_usuario = 2 }
    };

            var friendships = new List<amistades>(); // NO SON AMIGOS

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(friendships).Object);

            var repo = new FriendRepository(mockUserRepo.Object, () => mockDb.Object);

            // Act & Assert
            Assert.Throws<RepositoryValidationException>(() =>
                repo.DeleteFriend("A", "B")
            );
        }

        [Fact]
        public void DeleteFriend_UserDoesNotExist_ThrowsException()
        {
            // Arrange
            var mockUserRepo = new Mock<IRepositoryUsers>();
            mockUserRepo.Setup(r => r.GetUserIdByUsername("A")).Returns(1);
            mockUserRepo.Setup(r => r.GetUserIdByUsername("B")).Returns(999); // No existe en DB

            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1 },
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new FriendRepository(mockUserRepo.Object, () => mockDb.Object);

            // Act & Assert
            Assert.Throws<RepositoryValidationException>(() =>
                repo.DeleteFriend("A", "B")
            );
        }

        [Fact]
        public void DeleteFriend_FriendshipReverseOrder_RemovesCorrectEntry()
        {
            // Arrange
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

            // Act
            repo.DeleteFriend("A", "B");

            // Assert
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

            // Act
            repo.DeleteFriend("A", "B");

            // Assert
            mockDb.Verify(db => db.SaveChanges(), Times.Once);
        }

        [Fact]
        public void GetFriendPublicProfile_ValidUserWithProfile_ReturnsCorrectData()
        {
            // Arrange
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

            // Act
            var result = repo.GetFriendPublicProfile("seth");

            // Assert
            Assert.Equal("seth", result.Username);
            Assert.Equal("Seth", result.Name);
            Assert.Equal("Marquez", result.LastName);
            Assert.Equal("http://test.com", result.SocialUrl);
            Assert.Equal("img.png", result.AvatarFile);
            Assert.Equal(2, result.MatchesPlayed);
            Assert.Equal(1, result.Wins);
            Assert.Equal(1, result.Loses);
        }

        [Fact]
        public void GetFriendPublicProfile_UserWithoutProfile_UsesDefaultValues()
        {
            // Arrange
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("user")).Returns(1);

            var user = new usuarios
            {
                id_usuario = 1,
                perfiles = new List<perfiles>() // sin perfil
            };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<usuarios> { user }).Object);

            mockDb.Setup(db => db.participantes_partida)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<participantes_partida>()).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

            // Act
            var result = repo.GetFriendPublicProfile("user");

            // Assert
            Assert.Equal("", result.Username);
            Assert.Equal("", result.Name);
            Assert.Equal("", result.LastName);
            Assert.Equal("", result.SocialUrl);
            Assert.Equal("avatarIcon.png", result.AvatarFile);
        }

        [Fact]
        public void GetFriendPublicProfile_UserNotFound_ThrowsException()
        {
            // Arrange
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("ghost")).Returns(99);

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<usuarios>()).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

            // Act & Assert
            Assert.Throws<RepositoryValidationException>(() =>
                repo.GetFriendPublicProfile("ghost")
            );
        }

        [Fact]
        public void GetFriendPublicProfile_NoMatches_ShouldReturnZeroStats()
        {
            // Arrange
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

            // Act
            var result = repo.GetFriendPublicProfile("player");

            // Assert
            Assert.Equal(0, result.MatchesPlayed);
            Assert.Equal(0, result.Wins);
            Assert.Equal(0, result.Loses);
        }

        [Fact]
        public void GetFriendPublicProfile_NullAvatar_UsesDefaultAvatar()
        {
            // Arrange
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

            // Act
            var result = repo.GetFriendPublicProfile("pic");

            // Assert
            Assert.Equal("avatarIcon.png", result.AvatarFile);
        }


        [Fact]
        public void DeleteFriendAndBlock_AlreadyBlocked_ThrowsException()
        {
            // Arrange
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

            // Act & Assert
            Assert.Throws<RepositoryValidationException>(() =>
                repo.DeleteFriendAndBlock("A", "B")
            );
        }

        [Fact]
        public void DeleteFriendAndBlock_SameUser_ThrowsException()
        {
            // Arrange
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("A")).Returns(1);

            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios).Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

            // Act & Assert
            Assert.Throws<RepositoryValidationException>(() =>
                repo.DeleteFriendAndBlock("A", "A")
            );
        }

        [Fact]
        public void DeleteFriendAndBlock_UserNotFound_ThrowsException()
        {
            // Arrange
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("A")).Returns(1);
            mockRepo.Setup(r => r.GetUserIdByUsername("B")).Returns(2);

            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1 }
        // falta usuario 2
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios).Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

            // Act & Assert
            Assert.Throws<RepositoryValidationException>(() =>
                repo.DeleteFriendAndBlock("A", "B")
            );
        }

    }
}
