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
        public void FromCultureCodeNullReturnsEnglish()
        {
            var result = EmailLanguageMapper.FromCultureCode(null);

            Assert.Equal(EmailLanguage.English, result);
        }

        [Fact]
        public void FromCultureCodeEmptyReturnsEnglish()
        {
            var result = EmailLanguageMapper.FromCultureCode("");

            Assert.Equal(EmailLanguage.English, result);
        }

        [Fact]
        public void FromCultureCodeWhitespaceReturnsEnglish()
        {
            var result = EmailLanguageMapper.FromCultureCode("   ");

            Assert.Equal(EmailLanguage.English, result);
        }

        [Fact]
        public void FromCultureCodeSpanishCultureReturnsSpanish()
        {
            var result = EmailLanguageMapper.FromCultureCode("es-MX");

            Assert.Equal(EmailLanguage.Spanish, result);
        }

        [Fact]
        public void FromCultureCodePortugueseCultureReturnsPortuguese()
        {
            var result = EmailLanguageMapper.FromCultureCode("pt-BR");

            Assert.Equal(EmailLanguage.Portuguese, result);
        }

        [Fact]
        public void FromCultureCodeFrenchCultureReturnsFrench()
        {
            var result = EmailLanguageMapper.FromCultureCode("fr-FR");

            Assert.Equal(EmailLanguage.French, result);
        }

        [Fact]
        public void FromCultureCodeUnknownCultureReturnsEnglish()
        {
            var result = EmailLanguageMapper.FromCultureCode("de-DE");

            Assert.Equal(EmailLanguage.English, result);
        }
    }
}