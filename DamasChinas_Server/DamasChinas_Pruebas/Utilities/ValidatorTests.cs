using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Shared.Validation;
using Xunit;

namespace DamasChinas_Pruebas.Utilities
{
    public class ValidatorTests
    {
 
        [Fact]
        public void ValidateName_Empty_ThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateName("")
            );
        }

        [Fact]
        public void ValidateName_TooShort_ThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateName("A")
            );
        }

        [Fact]
        public void ValidateName_InvalidCharacters_ThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateName("Mar!o")
            );
        }

        [Fact]
        public void ValidateName_Valid_DoesNotThrow()
        {
            DamasChinas_Server.Utilidades.Validator.ValidateName("Seth");
        }


        [Fact]
        public void ValidateUsername_Empty_ThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateUsername("")
            );
        }

        [Fact]
        public void ValidateUsername_TooShort_ThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateUsername("a")
            );
        }

        [Fact]
        public void ValidateUsername_InvalidCharacters_ThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateUsername("seth!!")
            );
        }

        [Fact]
        public void ValidateUsername_Valid_DoesNotThrow()
        {
            DamasChinas_Server.Utilidades.Validator.ValidateUsername("sethMR");
        }


        [Fact]
        public void ValidatePassword_Empty_ThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidatePassword("")
            );
        }

        [Fact]
        public void ValidatePassword_NoUppercase_ThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidatePassword("hola123!")
            );
        }

        [Fact]
        public void ValidatePassword_NoLowercase_ThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidatePassword("HOLA123!")
            );
        }

        [Fact]
        public void ValidatePassword_NoDigit_ThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidatePassword("HolaMundo!")
            );
        }

        [Fact]
        public void ValidatePassword_NoSpecial_ThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidatePassword("Hola1234")
            );
        }

        [Fact]
        public void ValidatePassword_Valid_DoesNotThrow()
        {
            DamasChinas_Server.Utilidades.Validator.ValidatePassword("Hola123!");
        }


 
        [Fact]
        public void ValidateEmail_Empty_ThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateEmail("")
            );
        }

        [Fact]
        public void ValidateEmail_TooLong_ThrowsException()
        {
            var email = new string('a', UserValidationRules.EmailMaxLength + 1) + "@test.com";

            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateEmail(email)
            );
        }

        [Fact]
        public void ValidateEmail_InvalidFormat_ThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateEmail("seth@@@com")
            );
        }

        [Fact]
        public void ValidateEmail_Valid_DoesNotThrow()
        {
            DamasChinas_Server.Utilidades.Validator.ValidateEmail("seth@gmail.com");
        }


        [Fact]
        public void ValidateUserDto_Null_ThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateUserDto(null)
            );
        }

        [Fact]
        public void ValidateUserDto_InvalidName_ThrowsException()
        {
            var dto = new UserDto
            {
                Name = "",
                LastName = "Marquez",
                Email = "seth@mail.com",
                Username = "sethMR"
            };

            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateUserDto(dto)
            );
        }

        [Fact]
        public void ValidateUserDto_Valid_DoesNotThrow()
        {
            var dto = new UserDto
            {
                Name = "Seth",
                LastName = "Marquez",
                Email = "seth@mail.com",
                Username = "sethMR"
            };

            DamasChinas_Server.Utilidades.Validator.ValidateUserDto(dto);
        }

        [Fact]
        public void ValidateLoginRequest_Null_ThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateLoginRequest(null)
            );
        }

        [Fact]
        public void ValidateLoginRequest_EmptyUsername_ThrowsException()
        {
            var req = new LoginRequest { Username = "", Password = "123" };

            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateLoginRequest(req)
            );
        }

        [Fact]
        public void ValidateLoginRequest_BadEmail_ThrowsException()
        {
            var req = new LoginRequest { Username = "bademail@", Password = "123" };

            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateLoginRequest(req)
            );
        }

        [Fact]
        public void ValidateLoginRequest_Valid_DoesNotThrow()
        {
            var req = new LoginRequest { Username = "sethMR", Password = "Abc123!" };

            DamasChinas_Server.Utilidades.Validator.ValidateLoginRequest(req);
        }
    }
}
