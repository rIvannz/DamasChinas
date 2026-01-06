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
        public void SaveMessageValidMessageAddsMessage()
        {
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

            
            repo.Object.SaveMessage("seth", 99, "Hola mundo");

             

            Assert.Equal(5, mensajes[0].id_usuario_remitente);
            Assert.Equal(99, mensajes[0].id_usuario_destino);
            Assert.Equal("Hola mundo", mensajes[0].texto);
        }

        [Fact]
        public void SaveMessageValidMessageCallsSaveChangesOnce()
        {
             
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

             
            repo.Object.SaveMessage("seth", 3, "mensaje");

             
            mockDb.Verify(db => db.SaveChanges(), Times.Once);
        }

        [Fact]
        public void SaveMessageEmptyUsernameThrowsArgumentException()
        {
             
            var repo = new ChatRepository(() =>
            {
                var mockDb = new Mock<damas_chinasEntities>();
                mockDb.Setup(db => db.mensajes)
                      .Returns(MockDbSetHelper.CreateMockSet(new List<mensajes>()).Object);
                return mockDb.Object;
            });

               
            Assert.Throws<ArgumentException>(() =>
                repo.SaveMessage("", 2, "Hola")
            );
        }


        [Fact]
        public void SaveMessageUserNotFoundThrowsInvalidOperationException()
        {
             
            var usuarios = new List<usuarios>(); 

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios).Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new Mock<ChatRepository> { CallBase = true };
            repo.Setup(r => r.CreateDb()).Returns(mockDb.Object);

               
            Assert.Throws<InvalidOperationException>(() =>
                repo.Object.SaveMessage("ghost", 2, "hola")
            );
        }

        [Fact]
        public void GetChatByUsernameEmptySenderThrowsArgumentException()
        {
             
            var repo = new Mock<ChatRepository> { CallBase = true };

               
            Assert.Throws<ArgumentException>(() =>
                repo.Object.GetChatByUsername("", "dest")
            );
        }

        [Fact]
        public void GetChatByUsernameEmptyRecipientThrowsArgumentException()
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

               
            Assert.Throws<ArgumentException>(() =>
                mockRepo.Object.GetChatByUsername("sender", "")
            );
        }



        [Fact]
        public void GetChatByUsernameSenderNotFoundThrowsInvalidOperationException()
        {
             
            var usuarios = new List<usuarios>(); 

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new Mock<ChatRepository> { CallBase = true };
            repo.Setup(r => r.CreateDb()).Returns(mockDb.Object);

               
            Assert.Throws<InvalidOperationException>(() =>
                repo.Object.GetChatByUsername("ghost", "dest")
            );
        }

        [Fact]
        public void GetChatByUsernameRecipientNotFoundThrowsInvalidOperationException()
        {
             
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

               
            Assert.Throws<InvalidOperationException>(() =>
                repo.Object.GetChatByUsername("sender", "noExiste")
            );
        }

        [Fact]
        public void GetChatByUsernameNoMessagesReturnsEmptyList()
        {
             
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

             
            var result = repo.Object.GetChatByUsername("sender", "dest");

             
            Assert.Empty(result);
        }

        [Fact]
        public void GetChatByUsernameReturnsMessagesOrderedAndMappedCorrectly()
        {
             
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

             
            var result = repo.Object.GetChatByUsername("sender", "dest");


            Assert.True(
             result.Count == 2 &&

             result[0].Text == "Hola" &&
              result[0].UsarnameSender == "sender" &&
              result[0].DestinationUsername == "dest" &&

               result[1].Text == "Qué tal" &&
               result[1].UsarnameSender == "dest" &&
               result[1].DestinationUsername == "sender"
         );

        }
        [Fact]
        public void GetIdByUsernameNullUsernameThrowsArgumentException()
        {
             
            var repo = new Mock<ChatRepository> { CallBase = true };

               
            Assert.Throws<ArgumentException>(() =>
                repo.Object.GetIdByUsername(null)
            );
        }

        [Fact]
        public void GetIdByUsernameUserNotFoundThrowsInvalidOperationException()
        {
             
            var usuarios = new List<usuarios>(); 

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new Mock<ChatRepository> { CallBase = true };
            repo.Setup(r => r.CreateDb()).Returns(mockDb.Object);

               
            Assert.Throws<InvalidOperationException>(() =>
                repo.Object.GetIdByUsername("ghost")
            );
        }

        [Fact]
        public void GetIdByUsernameExactMatchReturnsUserId()
        {
             
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

             
            int id = repo.Object.GetIdByUsername("seth");

             
            Assert.Equal(10, id);
        }

    [Fact]
        public void GetIdByUsernameCaseInsensitiveMatchReturnsUserId()
        {
             
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

             
            int id = repo.Object.GetIdByUsername("playerone");

             
            Assert.Equal(20, id);
        }

        [Fact]
        public void GetIdByUsernameUserHasMultipleProfilesReturnsMatchingUserId()
        {
             
            var usuarios = new List<usuarios>
    {
        new usuarios
        {
            id_usuario = 33,
            perfiles = new List<perfiles>
            {
                new perfiles { username = "otro" },
                new perfiles { username = "seth" }  
            }
        }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new Mock<ChatRepository> { CallBase = true };
            repo.Setup(r => r.CreateDb()).Returns(mockDb.Object);

             
            int id = repo.Object.GetIdByUsername("seth");

             
            Assert.Equal(33, id);
        }

        [Fact]
        public void GetIdByUsernameOnlyOneUserMatchesReturnsCorrectId()
        {
             
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

             
            int id = repo.Object.GetIdByUsername("seth");

             
            Assert.Equal(55, id);
        }


    }
}
