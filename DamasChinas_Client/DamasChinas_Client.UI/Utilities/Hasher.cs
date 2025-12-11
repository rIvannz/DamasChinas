using System;
using System.Security.Cryptography;
using System.Text;
using static DamasChinas_Client.UI.Utilities.MessageKeys;
using DamasChinas_Client.UI.Utilities;

namespace DamasChinas_Client.UI.Utilities
{
    public static class Hasher
    {
        public static string HashPassword(string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
            {
                string msg = MessageTranslator.GetLocalizedMessage(InvalidPasswordEmpty);
                throw new ArgumentException(msg);
            }

            using (var sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(plainPassword);
                byte[] hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
