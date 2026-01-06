using DamasChinas_Server.Utilities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DamasChinas_Pruebas.Utilities
{
    public class EmailSenderTests
    {
        public EmailSenderTests()
        {
            EmailSender.Configure(new Mock<IEmailSender>().Object);
        }

        [Fact]
        public void ConfigureSetsEmailService()
        {
            var mock = new Mock<IEmailSender>();

            EmailSender.Configure(mock.Object);

            EmailSender.SendVerificationEmail("test@mail.com", "12345");
        }

        [Fact]
        public void SendVerificationEmailThrowsWhenNotConfigured()
        {
            typeof(EmailSender)
                .GetField("_emailService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .SetValue(null, null);

            Assert.Throws<InvalidOperationException>(() =>
                EmailSender.SendVerificationEmail("test@mail.com", "12345")
            );
        }

        [Fact]
        public void SendVerificationEmailCallsEmailServiceWithCorrectParameters()
        {
            var mock = new Mock<IEmailSender>();

            mock.Setup(s =>
                s.SendAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    true
                )
            ).ReturnsAsync(true);

            EmailSender.Configure(mock.Object);

            EmailSender.SendVerificationEmail("example@mail.com", "98765");

            mock.Verify(s =>
                s.SendAsync(
                    "example@mail.com",
                    "Código de verificación",
                    It.Is<string>(body =>
                        body.Contains("98765") &&
                        body.Contains("Este código expirará en 5 minutos.")
                    ),
                    true
                ),
                Times.Once
            );
        }

        [Fact]
        public void SendVerificationEmailThrowsWhenSenderThrows()
        {
            var mock = new Mock<IEmailSender>();

            mock.Setup(s =>
                s.SendAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    true
                )
            ).ThrowsAsync(new Exception("SMTP ERROR"));

            EmailSender.Configure(mock.Object);

            Assert.Throws<Exception>(() =>
                EmailSender.SendVerificationEmail("test@mail.com", "11111")
            );
        }
    }
}