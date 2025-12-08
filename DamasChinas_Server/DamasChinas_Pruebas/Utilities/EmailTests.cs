using DamasChinas_Server.Dtos;
using DamasChinas_Server.Utilities;
using Moq;
using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Xunit;

namespace DamasChinas_Pruebas.Utilities
{
    public class EmailTests
    {
        [Fact]
        public async Task SendAsync_ReturnsTrue_WhenSenderSucceeds()
        {
            var mock = new Mock<IEmailSender>();
            mock.Setup(s => s.SendAsync("mail@test.com", "Sub", "Body", true))
                .ReturnsAsync(true);

            var email = new Email(mock.Object);

            bool result = await email.SendAsync("mail@test.com", "Sub", "Body", true);

         
            Assert.True(result);
        }

        [Fact]
        public async Task SendAsync_ThrowsSmtpException_WhenSenderThrowsSmtpException()
        {
     
            var mock = new Mock<IEmailSender>();
            mock.Setup(s => s.SendAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    true))
                .Throws(new SmtpException("SMTP ERROR"));

            var email = new Email(mock.Object);

           
            await Assert.ThrowsAsync<SmtpException>(() =>
                email.SendAsync("mail@test.com", "S", "B", true)
            );
        }

        [Fact]
        public async Task SendAsync_ThrowsException_WhenSenderThrowsException()
        {
         
            var mock = new Mock<IEmailSender>();
            mock.Setup(s => s.SendAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    true))
                .Throws(new Exception("FAIL"));

            var email = new Email(mock.Object);

            await Assert.ThrowsAsync<Exception>(() =>
                email.SendAsync("mail@test.com", "S", "B", true)
            );
        }

        [Fact]
        public async Task SendWelcomeAsync_CallsSendAsync_WithCorrectParameters()
        {
            
            var mock = new Mock<IEmailSender>();
            mock.Setup(s => s.SendAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    true))
                .ReturnsAsync(true);

            var service = new Email(mock.Object);

            var user = new UserInfo
            {
                FullName = "Seth Márquez",
                Username = "SethMR",
                Email = "seth@mail.com"
            };

            await service.SendWelcomeAsync(user);

            
            mock.Verify(s => s.SendAsync(
                "seth@mail.com",
                "Bienvenido a Damas Chinas",
                It.Is<string>(body =>
                    body.Contains("Hola Seth Márquez") &&
                    body.Contains("<b>SethMR</b>")
                ),
                true
            ), Times.Once);
        }
    }
}
