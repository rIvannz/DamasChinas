using DamasChinas_Server;
using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.logic;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace DamasChinas_Pruebas.logic
{
    public class RepositoryUsersTests
    {

        [Fact]
        public void ValidateCreateUser_Throws_WhenEmailExists()
        {
            // Arrange
            var usuariosList = new List<usuarios>
            {
                new usuarios { correo = "existe@test.com" }
            };

            var perfilesList = new List<perfiles>();

            var mockUsuarios = MockDbSetHelper.CreateMockSet(usuariosList);
            var mockPerfiles = MockDbSetHelper.CreateMockSet(perfilesList);

            var mockDb = new Mock<IApplicationDbContext>();
            mockDb.Setup(db => db.usuarios).Returns(mockUsuarios.Object);
            mockDb.Setup(db => db.perfiles).Returns(mockPerfiles.Object);

            var repo = new RepositoryUsers(() => mockDb.Object);

            var dto = new UserDto
            {
                Email = "existe@test.com",
                Username = "nuevo"
            };

            // Act + Assert
            Assert.Throws<RepositoryValidationException>(() =>
                repo.ValidateCreateUser(dto));
        }


        [Fact]
        public void ValidateCreateUser_Throws_WhenUsernameExists()
        {
            // Arrange
            var usuariosList = new List<usuarios>();

            var perfilesList = new List<perfiles>
            {
                new perfiles { username = "Seth" }
            };

            var mockUsuarios = MockDbSetHelper.CreateMockSet(usuariosList);
            var mockPerfiles = MockDbSetHelper.CreateMockSet(perfilesList);

            var mockDb = new Mock<IApplicationDbContext>();
            mockDb.Setup(db => db.usuarios).Returns(mockUsuarios.Object);
            mockDb.Setup(db => db.perfiles).Returns(mockPerfiles.Object);

            var repo = new RepositoryUsers(() => mockDb.Object);

            var dto = new UserDto
            {
                Email = "otro@test.com",
                Username = "Seth"
            };

            // Act + Assert
            Assert.Throws<RepositoryValidationException>(() =>
                repo.ValidateCreateUser(dto));
        }


        [Fact]
        public void ValidateCreateUser_NoConflicts_DoesNotThrow()
        {
            // ARRANGE: 
            var usuariosList = new List<usuarios>();
            var perfilesList = new List<perfiles>();

            var mockUsuarios = MockDbSetHelper.CreateMockSet(usuariosList);
            var mockPerfiles = MockDbSetHelper.CreateMockSet(perfilesList);

            var mockDb = new Mock<IApplicationDbContext>();
            mockDb.Setup(db => db.usuarios).Returns(mockUsuarios.Object);
            mockDb.Setup(db => db.perfiles).Returns(mockPerfiles.Object);

            mockDb.Setup(db => db.Set<usuarios>()).Returns(mockUsuarios.Object);
            mockDb.Setup(db => db.Set<perfiles>()).Returns(mockPerfiles.Object);

            mockDb.Setup(db => db.SaveChanges()).Returns(1);

            
            var repo = new RepositoryUsers(() => mockDb.Object);

         
            var dto = new UserDto
            {
                Email = "nuevo@test.com",
                Username = "UsuarioNuevo",
                Name = "Seth",
                LastName = "Marquez",
                Password = "abc123"
            };

            // ACT
            var exception = Record.Exception(() => repo.ValidateCreateUser(dto));

            // ASSERT
            Assert.Null(exception);
        }


        [Fact]
        public void CreateUser_ValidUser_AddsUserAndProfile()
        {
            // ARRANGE
            var usuariosData = new List<usuarios>();
            var perfilesData = new List<perfiles>();

            var usuariosSet = MockDbSetHelper.CreateMockSet(usuariosData);
            var perfilesSet = MockDbSetHelper.CreateMockSet(perfilesData);

            var mockDb = new Mock<IApplicationDbContext>();

            mockDb.Setup(db => db.usuarios).Returns(usuariosSet.Object);
            mockDb.Setup(db => db.perfiles).Returns(perfilesSet.Object);

            mockDb.Setup(db => db.SaveChanges()).Callback(() =>
            {
                if (usuariosData.Count > 0 && usuariosData[0].id_usuario == 0)
                    usuariosData[0].id_usuario = 10; // simulate DB identity

                if (perfilesData.Count > 0 && perfilesData[0].id_usuario == 0)
                    perfilesData[0].id_usuario = usuariosData[0].id_usuario;
            });

            var repo = new RepositoryUsers(() => mockDb.Object);

            var dto = new UserDto
            {
                Email = "test@test.com",
                Username = "Usuario",
                Name = "Seth",
                LastName = "Marquez",
                Password = "abc123"
            };

            // ACT
            var result = repo.CreateUser(dto);

            // ASSERT
            Assert.Single(usuariosData);
            Assert.Single(perfilesData);

            Assert.Equal("test@test.com", usuariosData[0].correo);
            Assert.Equal("Usuario", perfilesData[0].username);

            Assert.Equal(10, result.id_usuario);   
        }


        [Fact]
        public void CreateUser_ReturnsUserWithProfileLoaded()
        {
            // ARRANGE
            var usuariosData = new List<usuarios>();
            var perfilesData = new List<perfiles>();

            var usuariosSet = MockDbSetHelper.CreateMockSet(usuariosData);
            var perfilesSet = MockDbSetHelper.CreateMockSet(perfilesData);

            var mockDb = new Mock<IApplicationDbContext>();

            mockDb.Setup(db => db.usuarios).Returns(usuariosSet.Object);
            mockDb.Setup(db => db.perfiles).Returns(perfilesSet.Object);

            mockDb.Setup(db => db.SaveChanges()).Callback(() =>
            {
                if (usuariosData.Count > 0 && usuariosData[0].id_usuario == 0)
                    usuariosData[0].id_usuario = 50;
            });

            var repo = new RepositoryUsers(() => mockDb.Object);

            var dto = new UserDto
            {
                Email = "user@test.com",
                Username = "Nuevo",
                Name = "Seth",
                LastName = "Rodriguez",
                Password = "12345"
            };

            // ACT
            var result = repo.CreateUser(dto);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(50, result.id_usuario);
        }

        [Fact]
        public void CreateUser_CallsSaveChangesTwice()
        {
            // ARRANGE
            var usuariosSet = MockDbSetHelper.CreateMockSet(new List<usuarios>());
            var perfilesSet = MockDbSetHelper.CreateMockSet(new List<perfiles>());

            var mockDb = new Mock<IApplicationDbContext>();

            mockDb.Setup(db => db.usuarios).Returns(usuariosSet.Object);
            mockDb.Setup(db => db.perfiles).Returns(perfilesSet.Object);

            int saveChangesCalls = 0;
            mockDb.Setup(db => db.SaveChanges()).Callback(() => saveChangesCalls++);

            var repo = new RepositoryUsers(() => mockDb.Object);

            var dto = new UserDto
            {
                Email = "test@test.com",
                Username = "NuevoUser",
                Name = "Seth",
                LastName = "Rod",
                Password = "pass123"
            };

            // ACT
            repo.CreateUser(dto);

            // ASSERT
            Assert.Equal(2, saveChangesCalls);  // User + Profile
        }

      
        [Fact]
        public void CreateUser_AssignsCorrectFields()
        {
            var usuariosData = new List<usuarios>();
            var perfilesData = new List<perfiles>();

            var usuariosSet = MockDbSetHelper.CreateMockSet(usuariosData);
            var perfilesSet = MockDbSetHelper.CreateMockSet(perfilesData);

            var mockDb = new Mock<IApplicationDbContext>();

            mockDb.Setup(db => db.usuarios).Returns(usuariosSet.Object);
            mockDb.Setup(db => db.perfiles).Returns(perfilesSet.Object);
            mockDb.Setup(db => db.SaveChanges());

            var repo = new RepositoryUsers(() => mockDb.Object);

            var dto = new UserDto
            {
                Email = "correo@test.com",
                Password = "pass123",
                Username = "Uno",
                Name = "Seth",
                LastName = "Mar"
            };

            // ACT
            repo.CreateUser(dto);

            // ASSERT
            Assert.Equal("correo@test.com", usuariosData[0].correo);
            Assert.Equal("pass123", usuariosData[0].password_hash);
        }



        [Fact]
        public void Login_ValidCredentials_ReturnsPublicProfile()
        {
            // Arrange
            var usuariosData = new List<usuarios>
            {
                new usuarios
                {
                    id_usuario = 1,
                    correo = "test@mail.com",
                    password_hash = "1234",
                    perfiles = new List<perfiles>
                    {
                        new perfiles
                        {
                            id_usuario = 1,
                            username = "Seth",
                            nombre = "Seth",
                            apellido_materno = "Marquez",
                            imagen_perfil = "avatar.png"
                        }
                    }
                }
            };

            var mockUsuarios = MockDbSetHelper.CreateMockSet(usuariosData);

            var mockDb = new Mock<IApplicationDbContext>();
            mockDb.Setup(db => db.usuarios).Returns(mockUsuarios.Object);
            mockDb.Setup(db => db.perfiles).Returns(MockDbSetHelper.CreateMockSet(new List<perfiles>()).Object);

            var repo = new RepositoryUsers(() => mockDb.Object);

            var request = new LoginRequest
            {
                Username = "Seth",
                Password = "1234"
            };

            // Act
            var profile = repo.Login(request);

            // Assert
            Assert.NotNull(profile);
            Assert.Equal(1, profile.IdUser);
            Assert.Equal("Seth", profile.Username);
            Assert.Equal("test@mail.com", profile.Email);
        }

        [Fact]
        public void Login_UserNotFound_ThrowsException()
        {
            // Arrange
            var mockDb = new Mock<IApplicationDbContext>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<usuarios>()).Object);

            mockDb.Setup(db => db.perfiles)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<perfiles>()).Object);

            var repo = new RepositoryUsers(() => mockDb.Object);

            var request = new LoginRequest
            {
                Username = "Unknown",
                Password = "1234"
            };

            // Act & Assert
            Assert.Throws<RepositoryValidationException>(() => repo.Login(request));
        }

        [Fact]
        public void Login_IncorrectPassword_ThrowsException()
        {
            // Arrange
            var usuariosData = new List<usuarios>
            {
                new usuarios
                {
                    id_usuario = 1,
                    correo = "test@mail.com",
                    password_hash = "correct",
                    perfiles = new List<perfiles>
                    {
                        new perfiles
                        {
                            id_usuario = 1,
                            username = "Seth"
                        }
                    }
                }
            };

            var mockDb = new Mock<IApplicationDbContext>();

            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuariosData).Object);
            mockDb.Setup(db => db.perfiles)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<perfiles>()).Object);

            var repo = new RepositoryUsers(() => mockDb.Object);

            var request = new LoginRequest
            {
                Username = "Seth",
                Password = "wrong"
            };

            // Act & Assert
            Assert.Throws<RepositoryValidationException>(() => repo.Login(request));
        }

        [Fact]
        public void Login_InvalidRequest_ThrowsValidationException()
        {
            var mockDb = new Mock<IApplicationDbContext>();
            var repo = new RepositoryUsers(() => mockDb.Object);

            var request = new LoginRequest
            {
                Username = "",
                Password = ""
            };

            Assert.Throws<RepositoryValidationException>(() => repo.Login(request));
        }


        [Fact]
        public void GetPublicProfile_UserExists_ReturnsPublicProfile()
        {
            // Arrange
            var usuariosData = new List<usuarios>
            {
                new usuarios
                {
                    id_usuario = 10,
                    correo = "test@correo.com",
                    perfiles = new List<perfiles>
                    {
                        new perfiles
                        {
                            id_usuario = 10,
                            username = "Seth",
                            nombre = "Seth",
                            apellido_materno = "Marquez",
                            imagen_perfil = "avatar.png"
                        }
                    }
                }
            };

            var mockUsuarios = MockDbSetHelper.CreateMockSet(usuariosData);

            var mockDb = new Mock<IApplicationDbContext>();
            mockDb.Setup(db => db.usuarios).Returns(mockUsuarios.Object);
            mockDb.Setup(db => db.perfiles).Returns(MockDbSetHelper.CreateMockSet(new List<perfiles>()).Object);

            var repo = new RepositoryUsers(() => mockDb.Object);

            // Act
            var profile = repo.GetPublicProfile(10);

            // Assert
            Assert.NotNull(profile);
            Assert.Equal(10, profile.IdUser);
            Assert.Equal("Seth", profile.Username);
            Assert.Equal("test@correo.com", profile.Email);
        }

        [Fact]
        public void GetPublicProfile_UserNotFound_ThrowsException()
        {
            // Arrange
            var mockDb = new Mock<IApplicationDbContext>();

            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<usuarios>()).Object);

            mockDb.Setup(db => db.perfiles)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<perfiles>()).Object);

            var repo = new RepositoryUsers(() => mockDb.Object);

            // Act & Assert
            Assert.Throws<RepositoryValidationException>(() => repo.GetPublicProfile(999));
        }

        [Fact]
        public void GetPublicProfile_UserExistsButHasNoProfile_ReturnsDefaultProfile()
        {
            // Arrange
            var usuariosData = new List<usuarios>
            {
                new usuarios
                {
                    id_usuario = 20,
                    correo = "noprof@mail.com",
                    perfiles = new List<perfiles>() // vacío
                }
            };

            var mockUsuarios = MockDbSetHelper.CreateMockSet(usuariosData);

            var mockDb = new Mock<IApplicationDbContext>();
            mockDb.Setup(db => db.usuarios).Returns(mockUsuarios.Object);
            mockDb.Setup(db => db.perfiles).Returns(MockDbSetHelper.CreateMockSet(new List<perfiles>()).Object);

            var repo = new RepositoryUsers(() => mockDb.Object);

            // Act
            var profile = repo.GetPublicProfile(20);

            // Assert
            Assert.NotNull(profile);
            Assert.Equal(20, profile.IdUser);
            Assert.Equal("N/A", profile.Username);
            Assert.Equal("N/A", profile.Name);
        }



        [Fact]
        public void GetFriendPublicProfile_ValidUsername_ReturnsProfile()
        {
            // Arrange
            var perfilesData = new List<perfiles>
            {
                new perfiles
                {
                    id_usuario = 1,
                    username = "Seth",
                    nombre = "Seth",
                    apellido_materno = "Marquez",
                    url = "https://perfil.test",
                    usuarios = new usuarios { id_usuario = 1, correo = "test@mail.com" }
                }
            };

            var mockPerfiles = MockDbSetHelper.CreateMockSet(perfilesData);

            var mockDb = new Mock<IApplicationDbContext>();
            mockDb.Setup(db => db.perfiles).Returns(mockPerfiles.Object);
            mockDb.Setup(db => db.usuarios).Returns(MockDbSetHelper.CreateMockSet(new List<usuarios>()).Object);

            var repo = new RepositoryUsers(() => mockDb.Object);

            // Act
            var result = repo.GetFriendPublicProfile("Seth");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Seth", result.Username);
            Assert.Equal("Seth", result.Name);
            Assert.Equal("Marquez", result.LastName);
            Assert.Equal("https://perfil.test", result.SocialUrl);
        }

        [Fact]
        public void GetFriendPublicProfile_EmptyUsername_ThrowsUsernameEmpty()
        {
            // Arrange
            var repo = new RepositoryUsers(() => new Mock<IApplicationDbContext>().Object);

            // Act & Assert
            Assert.Throws<RepositoryValidationException>(() => repo.GetFriendPublicProfile(""));
        }

        [Fact]
        public void GetFriendPublicProfile_UserNotFound_ThrowsUserProfileNotFound()
        {
            // Arrange
            var mockDb = new Mock<IApplicationDbContext>();

            mockDb.Setup(db => db.perfiles)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<perfiles>()).Object);

            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<usuarios>()).Object);

            var repo = new RepositoryUsers(() => mockDb.Object);

            // Act & Assert
            Assert.Throws<RepositoryValidationException>(() => repo.GetFriendPublicProfile("NoExiste"));
        }


        [Fact]
        public void ChangeUsername_EmptyCurrent_ThrowsUsernameEmpty()
        {
            var repo = new RepositoryUsers(() => Mock.Of<IApplicationDbContext>());
            Assert.Throws<RepositoryValidationException>(() => repo.ChangeUsername("   ", "Nuevo"));
        }

        [Fact]
        public void ChangeUsername_NewUsernameAlreadyExists_ThrowsUsernameExists()
        {
            var perfilesData = new List<perfiles>
        {
            new perfiles { username = "NuevoNombre" }
        };

            var mockDb = new Mock<IApplicationDbContext>();
            mockDb.Setup(db => db.perfiles)
                  .Returns(MockDbSetHelper.CreateMockSet(perfilesData).Object);

            mockDb.Setup(db => db.Set<perfiles>())
                  .Returns(MockDbSetHelper.CreateMockSet(perfilesData).Object);

            var repo = new RepositoryUsers(() => mockDb.Object);

            Assert.Throws<RepositoryValidationException>(
                () => repo.ChangeUsername("Actual", "NuevoNombre"));
        }

        [Fact]
        public void ChangeUsername_CurrentUserNotFound_ThrowsUserProfileNotFound()
        {
            var perfilesData = new List<perfiles>(); 

            var mockDb = new Mock<IApplicationDbContext>();
            mockDb.Setup(db => db.perfiles)
                  .Returns(MockDbSetHelper.CreateMockSet(perfilesData).Object);

            mockDb.Setup(db => db.Set<perfiles>())
                  .Returns(MockDbSetHelper.CreateMockSet(perfilesData).Object);

            var repo = new RepositoryUsers(() => mockDb.Object);

            Assert.Throws<RepositoryValidationException>(
                () => repo.ChangeUsername("NoExiste", "NuevoNombre"));
        }

        [Fact]
        public void ChangeUsername_ValidChange_ReturnsTrue()
        {
            var perfilesData = new List<perfiles>
        {
            new perfiles { username = "Actual", id_usuario = 1 }
        };

            var mockSet = MockDbSetHelper.CreateMockSet(perfilesData);

            var mockDb = new Mock<IApplicationDbContext>();
            mockDb.Setup(db => db.perfiles).Returns(mockSet.Object);
            mockDb.Setup(db => db.Set<perfiles>()).Returns(mockSet.Object);
            mockDb.Setup(db => db.SaveChanges()).Returns(1);

            var repo = new RepositoryUsers(() => mockDb.Object);

            var result = repo.ChangeUsername("Actual", "Nuevo");

            Assert.True(result);
            Assert.Equal("Nuevo", perfilesData[0].username);
            mockDb.Verify(db => db.SaveChanges(), Times.Once);
        }

        [Fact]
        public void ChangePassword_InvalidPassword_ThrowsValidationError()
        {
            var repo = new RepositoryUsers(() => Mock.Of<IApplicationDbContext>());

            Assert.Throws<RepositoryValidationException>(() =>
                repo.ChangePassword("user", "  "));
        }

        [Fact]
        public void ChangePassword_UserNotFound_ThrowsUserNotFound()
        {
            var usuarios = new List<usuarios>();
            var perfiles = new List<perfiles>(); 

            var mockDb = new Mock<IApplicationDbContext>();
            mockDb.Setup(db => db.usuarios).Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);
            mockDb.Setup(db => db.perfiles).Returns(MockDbSetHelper.CreateMockSet(perfiles).Object);

            var repo = new RepositoryUsers(() => mockDb.Object);

            Assert.Throws<RepositoryValidationException>(() =>
                repo.ChangePassword("Seth", "nueva123"));
        }

        [Fact]
        public void ChangePassword_ValidChange_ReturnsTrue()
        {
            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1, password_hash = "oldpass" }
    };

            var perfiles = new List<perfiles>
    {
        new perfiles { id_usuario = 1, username = "seth" } // minúscula
    };

            var mockUsuarios = MockDbSetHelper.CreateMockSet(usuarios);
            var mockPerfiles = MockDbSetHelper.CreateMockSet(perfiles);

            var mockDb = new Mock<IApplicationDbContext>();
            mockDb.Setup(db => db.usuarios).Returns(mockUsuarios.Object);
            mockDb.Setup(db => db.perfiles).Returns(mockPerfiles.Object);
            mockDb.Setup(db => db.SaveChanges()).Returns(1);

            var repo = new RepositoryUsers(() => mockDb.Object);

            var result = repo.ChangePassword("Seth", "Seth_Dev02");

            Assert.True(result);
            Assert.Equal("Seth_Dev02", usuarios[0].password_hash);
            mockDb.Verify(db => db.SaveChanges(), Times.Once);
        }


        [Fact]
        public void GetUserIdByUsername_EmptyUsername_ThrowsUsernameEmpty()
        {
            var mockDb = new Mock<IApplicationDbContext>();
            var repo = new RepositoryUsers(() => mockDb.Object);

            Assert.Throws<RepositoryValidationException>(() => repo.GetUserIdByUsername(""));
        }

        [Fact]
        public void GetUserIdByUsername_ProfileNotFound_ThrowsUserProfileNotFound()
        {
            var perfiles = new List<perfiles>();

            var mockPerfiles = MockDbSetHelper.CreateMockSet(perfiles);

            var mockDb = new Mock<IApplicationDbContext>();
            mockDb.Setup(db => db.perfiles).Returns(mockPerfiles.Object);

            var repo = new RepositoryUsers(() => mockDb.Object);

            Assert.Throws<RepositoryValidationException>(() => repo.GetUserIdByUsername("seth"));
        }

        [Fact]
        public void GetUserIdByUsername_ProfileExists_ReturnsId()
        {
            var perfiles = new List<perfiles>
    {
        new perfiles { id_usuario = 10, username = "Seth" }
    };

            var mockPerfiles = MockDbSetHelper.CreateMockSet(perfiles);

            var mockDb = new Mock<IApplicationDbContext>();
            mockDb.Setup(db => db.perfiles).Returns(mockPerfiles.Object);

            var repo = new RepositoryUsers(() => mockDb.Object);

            var result = repo.GetUserIdByUsername("Seth");

            Assert.Equal(10, result);
        }
        [Fact]
        public void ChangeAvatar_InvalidId_ThrowsUserNotFound()
        {
            var mockDb = new Mock<IApplicationDbContext>();
            var repo = new RepositoryUsers(() => mockDb.Object);

            Assert.Throws<RepositoryValidationException>(() => repo.ChangeAvatar(0, "img.png"));
        }

        [Fact]
        public void ChangeAvatar_EmptyAvatar_ThrowsAvatarUpdateFailed()
        {
            var mockDb = new Mock<IApplicationDbContext>();
            var repo = new RepositoryUsers(() => mockDb.Object);

            Assert.Throws<RepositoryValidationException>(() => repo.ChangeAvatar(5, ""));
        }

        [Fact]
        public void ChangeAvatar_ProfileNotFound_ThrowsUserProfileNotFound()
        {
            var perfiles = new List<perfiles>();

            var mockPerfiles = MockDbSetHelper.CreateMockSet(perfiles);

            var mockDb = new Mock<IApplicationDbContext>();
            mockDb.Setup(db => db.perfiles).Returns(mockPerfiles.Object);

            var repo = new RepositoryUsers(() => mockDb.Object);

            Assert.Throws<RepositoryValidationException>(() => repo.ChangeAvatar(10, "avatar.png"));
        }

        [Fact]
        public void ChangeAvatar_ValidUpdate_ReturnsTrue()
        {
            var perfiles = new List<perfiles>
    {
        new perfiles
        {
            id_usuario = 7,
            imagen_perfil = "old.png"
        }
    };

            var mockPerfiles = MockDbSetHelper.CreateMockSet(perfiles);

            var mockDb = new Mock<IApplicationDbContext>();
            mockDb.Setup(db => db.perfiles).Returns(mockPerfiles.Object);

            mockDb.Setup(db => db.SaveChanges());

            var repo = new RepositoryUsers(() => mockDb.Object);

            var result = repo.ChangeAvatar(7, "newAvatar.png");

            Assert.True(result);
            Assert.Equal("newAvatar.png", perfiles[0].imagen_perfil);
            mockDb.Verify(db => db.SaveChanges(), Times.Once);
        }
        public static class PrivateAccessor
        {
            public static object CallPrivate(Type type, string methodName, params object[] parameters)
            {
                var method = type.GetMethod(
                    methodName,
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Static);

                if (method == null)
                    throw new MissingMethodException($"Método privado '{methodName}' no encontrado.");

                return method.Invoke(null, parameters);
            }
        }

        [Fact]
        public void CreateUsuario_CreatesAndAddsUser()
        {
            var list = new List<usuarios>();
            var mock = new Mock<IApplicationDbContext>();
            mock.Setup(db => db.usuarios).Returns(MockDbSetHelper.CreateMockSet(list).Object);

            var dto = new UserDto { Email = "test@mail.com", Password = "123" };

            var result = (usuarios)PrivateAccessor.CallPrivate(
                typeof(RepositoryUsers), "CreateUsuario", mock.Object, dto);

            Assert.Single(list);
            Assert.Equal("test@mail.com", result.correo);
        }

        [Fact]
        public void CreateUsuario_CallsSaveChanges()
        {
            var mock = new Mock<IApplicationDbContext>();
            mock.Setup(db => db.usuarios).Returns(MockDbSetHelper.CreateMockSet(new List<usuarios>()).Object);

            var dto = new UserDto { Email = "x@mail.com", Password = "abc" };

            PrivateAccessor.CallPrivate(typeof(RepositoryUsers), "CreateUsuario", mock.Object, dto);

            mock.Verify(db => db.SaveChanges(), Times.Once);
        }


        [Fact]
        public void CreatePerfil_AddsProfile()
        {
            var list = new List<perfiles>();
            var mock = new Mock<IApplicationDbContext>();
            mock.Setup(db => db.perfiles).Returns(MockDbSetHelper.CreateMockSet(list).Object);

            var usuario = new usuarios { id_usuario = 1 };
            var dto = new UserDto { Username = "User", Name = "Seth", LastName = "M" };

            PrivateAccessor.CallPrivate(typeof(RepositoryUsers), "CreatePerfil", mock.Object, usuario, dto);

            Assert.Single(list);
        }

        [Fact]
        public void CreatePerfil_CallsSaveChanges()
        {
            var mock = new Mock<IApplicationDbContext>();
            mock.Setup(db => db.perfiles).Returns(MockDbSetHelper.CreateMockSet(new List<perfiles>()).Object);

            var usuario = new usuarios { id_usuario = 3 };
            var dto = new UserDto { Username = "A", Name = "B", LastName = "C" };

            PrivateAccessor.CallPrivate(typeof(RepositoryUsers), "CreatePerfil", mock.Object, usuario, dto);

            mock.Verify(db => db.SaveChanges(), Times.Once);
        }

        [Fact]
        public void GetUserWithProfile_ReturnsCorrectUser()
        {
            var list = new List<usuarios>
            {
                new usuarios { id_usuario = 9, perfiles = new List<perfiles>() }
            };

            var mock = new Mock<IApplicationDbContext>();
            mock.Setup(db => db.usuarios).Returns(MockDbSetHelper.CreateMockSet(list).Object);

            var result = (usuarios)PrivateAccessor.CallPrivate(
                typeof(RepositoryUsers), "GetUserWithProfile", mock.Object, 9);

            Assert.Equal(9, result.id_usuario);
        }


        [Fact]
        public void GetPerfilByUsername_Found_ReturnsPerfil()
        {
            var list = new List<perfiles> { new perfiles { username = "Seth" } };

            var mock = new Mock<IApplicationDbContext>();
            mock.Setup(db => db.perfiles).Returns(MockDbSetHelper.CreateMockSet(list).Object);

            var result = (perfiles)PrivateAccessor.CallPrivate(
                typeof(RepositoryUsers), "GetPerfilByUsername",
                mock.Object, "Seth");

            Assert.Equal("Seth", result.username);
        }


        [Fact]
        public void FindUserForLogin_ByEmail_ReturnsUser()
        {
            var list = new List<usuarios>
            {
                new usuarios { correo = "mail@test.com", perfiles = new List<perfiles>() }
            };

            var mock = new Mock<IApplicationDbContext>();
            mock.Setup(db => db.usuarios).Returns(MockDbSetHelper.CreateMockSet(list).Object);

            var result = (usuarios)PrivateAccessor.CallPrivate(
                typeof(RepositoryUsers), "FindUserForLogin", mock.Object, "mail@test.com");

            Assert.NotNull(result);
        }

        [Fact]
        public void FindUserForLogin_ByUsername_ReturnsUser()
        {
            var list = new List<usuarios>
            {
                new usuarios
                {
                    correo = "x@mail.com",
                    perfiles = new List<perfiles>
                    {
                        new perfiles { username = "Seth" }
                    }
                }
            };

            var mock = new Mock<IApplicationDbContext>();
            mock.Setup(db => db.usuarios).Returns(MockDbSetHelper.CreateMockSet(list).Object);

            var result = (usuarios)PrivateAccessor.CallPrivate(
                typeof(RepositoryUsers), "FindUserForLogin", mock.Object, "Seth");

            Assert.NotNull(result);
        }

        [Fact]
        public void SendFriendRequest_ValidRequest_AddsRequestAndReturnsTrue()
        {
            // Arrange
            var mockUserRepo = new Mock<IRepositoryUsers>();
            mockUserRepo.Setup(r => r.GetUserIdByUsername("sender")).Returns(1);
            mockUserRepo.Setup(r => r.GetUserIdByUsername("receiver")).Returns(2);

            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1 },
        new usuarios { id_usuario = 2 }
    };

            var solicitudes = new List<solicitudes_amistad>();

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<amistades>()).Object);

            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<bloqueos>()).Object);

            var repo = new FriendRepository(mockUserRepo.Object, () => mockDb.Object);

            // Act
            var result = repo.SendFriendRequest("sender", "receiver");

            // Assert
            Assert.True(result);
            Assert.Single(solicitudes); //<--- YA NO FALLA
        }

        [Fact]
        public void SendFriendRequest_UserDoesNotExist_ThrowsUserNotFound()
        {
            // Arrange
            var mockUserRepo = new Mock<IRepositoryUsers>();
            mockUserRepo.Setup(r => r.GetUserIdByUsername("sender")).Returns(1);
            mockUserRepo.Setup(r => r.GetUserIdByUsername("receiver")).Returns(2);

            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1 }
        // Falta el usuario 2
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios).Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new FriendRepository(mockUserRepo.Object, () => mockDb.Object);

            // Act & Assert
            Assert.Throws<RepositoryValidationException>(() =>
                repo.SendFriendRequest("sender", "receiver")
            );
        }

        [Fact]
        public void SendFriendRequest_AlreadyFriends_ThrowsFriendsLoadError()
        {
            // Arrange
            var mockUserRepo = new Mock<IRepositoryUsers>();
            mockUserRepo.Setup(r => r.GetUserIdByUsername("sender")).Returns(1);
            mockUserRepo.Setup(r => r.GetUserIdByUsername("receiver")).Returns(2);

            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1 },
        new usuarios { id_usuario = 2 }
    };

            var amistades = new List<amistades>
    {
        new amistades { id_usuario1 = 1, id_usuario2 = 2 }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios).Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);
            mockDb.Setup(db => db.amistades).Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

            var repo = new FriendRepository(mockUserRepo.Object, () => mockDb.Object);

            // Act & Assert
            Assert.Throws<RepositoryValidationException>(() =>
                repo.SendFriendRequest("sender", "receiver")
            );
        }

        [Fact]
        public void SendFriendRequest_ReceiverHasBlockedSender_ThrowsFriendsLoadError()
        {
            // Arrange
            var mockUserRepo = new Mock<IRepositoryUsers>();
            mockUserRepo.Setup(r => r.GetUserIdByUsername("sender")).Returns(1);
            mockUserRepo.Setup(r => r.GetUserIdByUsername("receiver")).Returns(2);

            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1 },
        new usuarios { id_usuario = 2 }
    };

            var bloqueos = new List<bloqueos>
    {
        new bloqueos { id_bloqueador = 2, id_bloqueado = 1 } // receiver bloqueó sender
    };

            var mockDb = new Mock<damas_chinasEntities>();

            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<amistades>()).Object);

            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<solicitudes_amistad>()).Object);

            var repo = new FriendRepository(mockUserRepo.Object, () => mockDb.Object);

            // Act & Assert
            Assert.Throws<RepositoryValidationException>(() =>
                repo.SendFriendRequest("sender", "receiver")
            );
        }

        [Fact]
        public void SendFriendRequest_PendingRequestExists_ThrowsFriendsLoadError()
        {
            // Arrange
            var mockUserRepo = new Mock<IRepositoryUsers>();
            mockUserRepo.Setup(r => r.GetUserIdByUsername("sender")).Returns(1);
            mockUserRepo.Setup(r => r.GetUserIdByUsername("receiver")).Returns(2);

            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1 },
        new usuarios { id_usuario = 2 }
    };

            var pending = new List<solicitudes_amistad>
    {
        new solicitudes_amistad
        {
            id_emisor = 1,
            id_receptor = 2,
            estado = "pendiente"
        }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(pending).Object);

            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<amistades>()).Object);

            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(new List<bloqueos>()).Object);

            var repo = new FriendRepository(mockUserRepo.Object, () => mockDb.Object);

            // Act + Assert
            Assert.Throws<RepositoryValidationException>(() =>
                repo.SendFriendRequest("sender", "receiver")
            );
        }
        [Fact]
        public void UpdateFriendRequestStatus_NoPendingRequest_ThrowsException()
        {
            // Arrange
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("receiver")).Returns(1);
            mockRepo.Setup(r => r.GetUserIdByUsername("sender")).Returns(2);

            var solicitudes = new List<solicitudes_amistad>(); // no hay solicitudes

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

            // Act & Assert
            Assert.Throws<RepositoryValidationException>(() =>
                repo.UpdateFriendRequestStatus("receiver", "sender", true)
            );
        }
        [Fact]
        public void UpdateFriendRequestStatus_RequestNotPending_ThrowsException()
        {
            // Arrange
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("receiver")).Returns(1);
            mockRepo.Setup(r => r.GetUserIdByUsername("sender")).Returns(2);

            var solicitudes = new List<solicitudes_amistad>
    {
        new solicitudes_amistad { id_emisor = 2, id_receptor = 1, estado = "rechazada" }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

            // Act & Assert
            Assert.Throws<RepositoryValidationException>(() =>
                repo.UpdateFriendRequestStatus("receiver", "sender", true)
            );
        }

        [Fact]
        public void UpdateFriendRequestStatus_Accepted_AddsFriendship()
        {
            // Arrange
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("receiver")).Returns(10);
            mockRepo.Setup(r => r.GetUserIdByUsername("sender")).Returns(20);

            var solicitudes = new List<solicitudes_amistad>
    {
        new solicitudes_amistad { id_emisor = 20, id_receptor = 10, estado = "pendiente" }
    };

            var amistades = new List<amistades>();
            var bloqueos = new List<bloqueos>();

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);
            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

            // Act
            repo.UpdateFriendRequestStatus("receiver", "sender", true);

            // Assert
            Assert.Single(amistades);
            Assert.Empty(solicitudes);
        }

        [Fact]
        public void UpdateFriendRequestStatus_Accepted_AlreadyFriends_DoesNotDuplicate()
        {
            // Arrange
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("receiver")).Returns(10);
            mockRepo.Setup(r => r.GetUserIdByUsername("sender")).Returns(20);

            var solicitudes = new List<solicitudes_amistad>
    {
        new solicitudes_amistad { id_emisor = 20, id_receptor = 10, estado = "pendiente" }
    };

            var amistades = new List<amistades>
    {
        new amistades { id_usuario1 = 20, id_usuario2 = 10 }
    };

            var bloqueos = new List<bloqueos>();

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);
            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

            // Act
            repo.UpdateFriendRequestStatus("receiver", "sender", true);

            // Assert
            Assert.Single(amistades);  
            Assert.Empty(solicitudes);  
        }

        [Fact]
        public void UpdateFriendRequestStatus_AcceptBlocked_ThrowsException()
        {
            // Arrange
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("receiver")).Returns(10);
            mockRepo.Setup(r => r.GetUserIdByUsername("sender")).Returns(20);

            var solicitudes = new List<solicitudes_amistad>
    {
        new solicitudes_amistad { id_emisor = 20, id_receptor = 10, estado = "pendiente" }
    };

            var bloqueos = new List<bloqueos>
    {
        new bloqueos { id_bloqueador = 20, id_bloqueado = 10 }
    };

            var amistades = new List<amistades>();

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);
            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

            // Act & Assert
            Assert.Throws<RepositoryValidationException>(() =>
                repo.UpdateFriendRequestStatus("receiver", "sender", true)
            );
        }
        [Fact]
        public void UpdateFriendRequestStatus_Rejected_ChangesState()
        {
            // Arrange
            var mockRepo = new Mock<IRepositoryUsers>();
            mockRepo.Setup(r => r.GetUserIdByUsername("receiver")).Returns(1);
            mockRepo.Setup(r => r.GetUserIdByUsername("sender")).Returns(2);

            var solicitudes = new List<solicitudes_amistad>
    {
        new solicitudes_amistad { id_emisor = 2, id_receptor = 1, estado = "pendiente" }
    };

            var amistades = new List<amistades>();
            var bloqueos = new List<bloqueos>();

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.solicitudes_amistad)
                  .Returns(MockDbSetHelper.CreateMockSet(solicitudes).Object);
            mockDb.Setup(db => db.amistades)
                  .Returns(MockDbSetHelper.CreateMockSet(amistades).Object);
            mockDb.Setup(db => db.bloqueos)
                  .Returns(MockDbSetHelper.CreateMockSet(bloqueos).Object);

            var repo = new FriendRepository(mockRepo.Object, () => mockDb.Object);

            // Act
            repo.UpdateFriendRequestStatus("receiver", "sender", false);

            // Assert
            Assert.Equal("rechazada", solicitudes[0].estado);
        }



    }
}