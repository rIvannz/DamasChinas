using DamasChinas_Server;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamasChinas_Pruebas.logic
{
    public class ChatRepositoryTests
    {
        [Fact]
        public void SaveMessage_ValidMessage_AddsMessage()
        {
            // Arrange
            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 5, perfiles = new List<perfiles>{ new perfiles { username = "seth" } } }
    };

            var mensajes = new List<mensajes>();

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios).Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);
            mockDb.Setup(db => db.mensajes).Returns(MockDbSetHelper.CreateMockSet(mensajes).Object);

            var repo = new Mock<ChatRepository> { CallBase = true };
            repo.Setup(r => r.CreateDb()).Returns(mockDb.Object);

            // Act
            repo.Object.SaveMessage("seth", 99, "Hola mundo");

            // Assert
            Assert.Single(mensajes);
            Assert.Equal(5, mensajes[0].id_usuario_remitente);
            Assert.Equal(99, mensajes[0].id_usuario_destino);
            Assert.Equal("Hola mundo", mensajes[0].texto);
        }

        [Fact]
        public void SaveMessage_ValidMessage_CallsSaveChangesOnce()
        {
            // Arrange
            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 10, perfiles = new List<perfiles>{ new perfiles { username = "seth" } } }
    };

            var mensajes = new List<mensajes>();

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios).Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);
            mockDb.Setup(db => db.mensajes).Returns(MockDbSetHelper.CreateMockSet(mensajes).Object);

            mockDb.Setup(db => db.SaveChanges()).Verifiable();

            var repo = new Mock<ChatRepository> { CallBase = true };
            repo.Setup(r => r.CreateDb()).Returns(mockDb.Object);

            // Act
            repo.Object.SaveMessage("seth", 3, "mensaje");

            // Assert
            mockDb.Verify(db => db.SaveChanges(), Times.Once);
        }

        [Fact]
        public void SaveMessage_EmptyUsername_ThrowsArgumentException()
        {
            // Arrange
            var repo = new ChatRepository(() =>
            {
                var mockDb = new Mock<damas_chinasEntities>();
                mockDb.Setup(db => db.mensajes)
                      .Returns(MockDbSetHelper.CreateMockSet(new List<mensajes>()).Object);
                return mockDb.Object;
            });

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                repo.SaveMessage("", 2, "Hola")
            );
        }


        [Fact]
        public void SaveMessage_UserNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var usuarios = new List<usuarios>(); // vacío → usuario no existe

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios).Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new Mock<ChatRepository> { CallBase = true };
            repo.Setup(r => r.CreateDb()).Returns(mockDb.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                repo.Object.SaveMessage("ghost", 2, "hola")
            );
        }

        [Fact]
        public void GetChatByUsername_EmptySender_ThrowsArgumentException()
        {
            // Arrange
            var repo = new Mock<ChatRepository> { CallBase = true };

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                repo.Object.GetChatByUsername("", "dest")
            );
        }

        [Fact]
        public void GetChatByUsername_EmptyRecipient_ThrowsArgumentException()
        {
            var usuarios = new List<usuarios>
    {
        new usuarios
        {
            id_usuario = 1,
            perfiles = new List<perfiles>
            {
                new perfiles { username = "sender" }
            }
        }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var mockRepo = new Mock<ChatRepository> { CallBase = true };
            mockRepo.Setup(r => r.CreateDb()).Returns(mockDb.Object);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                mockRepo.Object.GetChatByUsername("sender", "")
            );
        }



        [Fact]
        public void GetChatByUsername_SenderNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var usuarios = new List<usuarios>(); // no existe sender

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new Mock<ChatRepository> { CallBase = true };
            repo.Setup(r => r.CreateDb()).Returns(mockDb.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                repo.Object.GetChatByUsername("ghost", "dest")
            );
        }

        [Fact]
        public void GetChatByUsername_RecipientNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var usuarios = new List<usuarios>
    {
        new usuarios
        {
            id_usuario = 1,
            perfiles = new List<perfiles> { new perfiles { username = "sender" } }
        }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new Mock<ChatRepository> { CallBase = true };
            repo.Setup(r => r.CreateDb()).Returns(mockDb.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                repo.Object.GetChatByUsername("sender", "noExiste")
            );
        }

        [Fact]
        public void GetChatByUsername_NoMessages_ReturnsEmptyList()
        {
            // Arrange
            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1, perfiles = new List<perfiles> { new perfiles { username = "sender" } }},
        new usuarios { id_usuario = 2, perfiles = new List<perfiles> { new perfiles { username = "dest" } }}
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            mockDb.Setup(db => db.mensajes)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<mensajes>()).Object);

            var repo = new Mock<ChatRepository> { CallBase = true };
            repo.Setup(r => r.CreateDb()).Returns(mockDb.Object);

            // Act
            var result = repo.Object.GetChatByUsername("sender", "dest");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetChatByUsername_ReturnsMessagesOrderedAndMappedCorrectly()
        {
            // Arrange
            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1, perfiles = new List<perfiles> { new perfiles { username = "sender" }}},
        new usuarios { id_usuario = 2, perfiles = new List<perfiles> { new perfiles { username = "dest" }}}
    };

            var mensajes = new List<mensajes>
    {
        new mensajes
        {
            id_usuario_remitente = 1,
            id_usuario_destino = 2,
            texto = "Hola",
            fecha_envio = new DateTime(2024,1,1)
        },
        new mensajes
        {
            id_usuario_remitente = 2,
            id_usuario_destino = 1,
            texto = "Qué tal",
            fecha_envio = new DateTime(2024,1,2)
        }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            mockDb.Setup(db => db.mensajes)
                  .Returns(MockDbSetHelper.CreateMockSet(mensajes).Object);

            var repo = new Mock<ChatRepository> { CallBase = true };
            repo.Setup(r => r.CreateDb()).Returns(mockDb.Object);

            // Act
            var result = repo.Object.GetChatByUsername("sender", "dest");

            // Assert
            Assert.Equal(2, result.Count);

            Assert.Equal("Hola", result[0].Text);
            Assert.Equal("sender", result[0].UsarnameSender);
            Assert.Equal("dest", result[0].DestinationUsername);

            Assert.Equal("Qué tal", result[1].Text);
            Assert.Equal("dest", result[1].UsarnameSender);
            Assert.Equal("sender", result[1].DestinationUsername);
        }
        [Fact]
        public void GetIdByUsername_NullUsername_ThrowsArgumentException()
        {
            // Arrange
            var repo = new Mock<ChatRepository> { CallBase = true };

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                repo.Object.GetIdByUsername(null)
            );
        }

        [Fact]
        public void GetIdByUsername_UserNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var usuarios = new List<usuarios>(); // vacío → no existe nadie

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new Mock<ChatRepository> { CallBase = true };
            repo.Setup(r => r.CreateDb()).Returns(mockDb.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                repo.Object.GetIdByUsername("ghost")
            );
        }

        [Fact]
        public void GetIdByUsername_ExactMatch_ReturnsUserId()
        {
            // Arrange
            var usuarios = new List<usuarios>
    {
        new usuarios
        {
            id_usuario = 10,
            perfiles = new List<perfiles> { new perfiles { username = "seth" } }
        }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new Mock<ChatRepository> { CallBase = true };
            repo.Setup(r => r.CreateDb()).Returns(mockDb.Object);

            // Act
            int id = repo.Object.GetIdByUsername("seth");

            // Assert
            Assert.Equal(10, id);
        }

    [Fact]
        public void GetIdByUsername_CaseInsensitiveMatch_ReturnsUserId()
        {
            // Arrange
            var usuarios = new List<usuarios>
    {
        new usuarios
        {
            id_usuario = 20,
            perfiles = new List<perfiles>
            {
                new perfiles { username = "PlayerOne" }
            }
        }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new Mock<ChatRepository> { CallBase = true };
            repo.Setup(r => r.CreateDb()).Returns(mockDb.Object);

            // Act
            int id = repo.Object.GetIdByUsername("playerone");

            // Assert
            Assert.Equal(20, id);
        }

        [Fact]
        public void GetIdByUsername_UserHasMultipleProfiles_ReturnsMatchingUserId()
        {
            // Arrange
            var usuarios = new List<usuarios>
    {
        new usuarios
        {
            id_usuario = 33,
            perfiles = new List<perfiles>
            {
                new perfiles { username = "otro" },
                new perfiles { username = "seth" }  // ← este coincide
            }
        }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new Mock<ChatRepository> { CallBase = true };
            repo.Setup(r => r.CreateDb()).Returns(mockDb.Object);

            // Act
            int id = repo.Object.GetIdByUsername("seth");

            // Assert
            Assert.Equal(33, id);
        }

        [Fact]
        public void GetIdByUsername_OnlyOneUserMatches_ReturnsCorrectId()
        {
            // Arrange
            var usuarios = new List<usuarios>
    {
        new usuarios
        {
            id_usuario = 1,
            perfiles = new List<perfiles>{ new perfiles { username = "juan" } }
        },
        new usuarios
        {
            id_usuario = 55,
            perfiles = new List<perfiles>{ new perfiles { username = "seth" } }
        }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new Mock<ChatRepository> { CallBase = true };
            repo.Setup(r => r.CreateDb()).Returns(mockDb.Object);

            // Act
            int id = repo.Object.GetIdByUsername("seth");

            // Assert
            Assert.Equal(55, id);
        }


    }
}
