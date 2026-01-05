using DamasChinas_Server.Utilities.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamasChinas_Tests.Utilities
{
    public class EmailLanguageMapperTests
    {
        [Fact]
        public void FromCultureCode_Null_ReturnsEnglish()
        {
            var result = EmailLanguageMapper.FromCultureCode(null);

            Assert.Equal(EmailLanguage.English, result);
        }

        [Fact]
        public void FromCultureCode_Empty_ReturnsEnglish()
        {
            var result = EmailLanguageMapper.FromCultureCode("");

            Assert.Equal(EmailLanguage.English, result);
        }

        [Fact]
        public void FromCultureCode_Whitespace_ReturnsEnglish()
        {
            var result = EmailLanguageMapper.FromCultureCode("   ");

            Assert.Equal(EmailLanguage.English, result);
        }

        [Fact]
        public void FromCultureCode_SpanishCulture_ReturnsSpanish()
        {
            var result = EmailLanguageMapper.FromCultureCode("es-MX");

            Assert.Equal(EmailLanguage.Spanish, result);
        }

        [Fact]
        public void FromCultureCode_PortugueseCulture_ReturnsPortuguese()
        {
            var result = EmailLanguageMapper.FromCultureCode("pt-BR");

            Assert.Equal(EmailLanguage.Portuguese, result);
        }

        [Fact]
        public void FromCultureCode_FrenchCulture_ReturnsFrench()
        {
            var result = EmailLanguageMapper.FromCultureCode("fr-FR");

            Assert.Equal(EmailLanguage.French, result);
        }

        [Fact]
        public void FromCultureCode_UnknownCulture_ReturnsEnglish()
        {
            var result = EmailLanguageMapper.FromCultureCode("de-DE");

            Assert.Equal(EmailLanguage.English, result);
        }
    }
}