using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Shared.Validation;
using Xunit;

namespace DamasChinas_Pruebas.Utilities
{
    public class ValidatorTests
    {
 
        [Fact]
        public void ValidateNameEmptyThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateName("")
            );
        }

        [Fact]
        public void ValidateNameTooShortThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateName("A")
            );
        }

        [Fact]
        public void ValidateNameInvalidCharactersThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateName("Mar!o")
            );
        }

        [Fact]
        public void ValidateNameValidDoesNotThrow()
        {
            DamasChinas_Server.Utilidades.Validator.ValidateName("Seth");
        }


        [Fact]
        public void ValidateUsernameEmptyThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateUsername("")
            );
        }

        [Fact]
        public void ValidateUsernameTooShortThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateUsername("a")
            );
        }

        [Fact]
        public void ValidateUsernameInvalidCharactersThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateUsername("seth!!")
            );
        }

        [Fact]
        public void ValidateUsernameValidDoesNotThrow()
        {
            DamasChinas_Server.Utilidades.Validator.ValidateUsername("sethMR");
        }


        [Fact]
        public void ValidatePasswordEmptyThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidatePassword("")
            );
        }

        [Fact]
        public void ValidatePasswordNoUppercaseThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidatePassword("hola123!")
            );
        }

        [Fact]
        public void ValidatePasswordNoLowercaseThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidatePassword("HOLA123!")
            );
        }

        [Fact]
        public void ValidatePasswordNoDigitThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidatePassword("HolaMundo!")
            );
        }

        [Fact]
        public void ValidatePasswordNoSpecialThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidatePassword("Hola1234")
            );
        }

        [Fact]
        public void ValidatePasswordValidDoesNotThrow()
        {
            DamasChinas_Server.Utilidades.Validator.ValidatePassword("Hola123!");
        }


 
        [Fact]
        public void ValidateEmailEmptyThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateEmail("")
            );
        }

        [Fact]
        public void ValidateEmailTooLongThrowsException()
        {
            var email = new string('a', UserValidationRules.EmailMaxLength + 1) + "@test.com";

            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateEmail(email)
            );
        }

        [Fact]
        public void ValidateEmailInvalidFormatThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateEmail("seth@@@com")
            );
        }

        [Fact]
        public void ValidateEmailValidDoesNotThrow()
        {
            DamasChinas_Server.Utilidades.Validator.ValidateEmail("seth@gmail.com");
        }


        [Fact]
        public void ValidateUserDtoNullThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateUserDto(null)
            );
        }

        [Fact]
        public void ValidateUserDtoInvalidNameThrowsException()
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
        public void ValidateUserDtoValidDoesNotThrow()
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
        public void ValidateLoginRequestNullThrowsException()
        {
            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateLoginRequest(null)
            );
        }

        [Fact]
        public void ValidateLoginRequestEmptyUsernameThrowsException()
        {
            var req = new LoginRequest { Username = "", Password = "123" };

            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateLoginRequest(req)
            );
        }

        [Fact]
        public void ValidateLoginRequestBadEmailThrowsException()
        {
            var req = new LoginRequest { Username = "bademail@", Password = "123" };

            Assert.Throws<RepositoryValidationException>(
                () => DamasChinas_Server.Utilidades.Validator.ValidateLoginRequest(req)
            );
        }

        [Fact]
        public void ValidateLoginRequestValidDoesNotThrow()
        {
            var req = new LoginRequest { Username = "sethMR", Password = "Abc123!" };

            DamasChinas_Server.Utilidades.Validator.ValidateLoginRequest(req);
        }
    }
}
