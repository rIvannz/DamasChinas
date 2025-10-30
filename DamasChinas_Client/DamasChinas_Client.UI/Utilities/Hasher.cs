using System;
using System.Security.Cryptography;
using System.Text;

namespace DamasChinas_Client.UI.Utilities
{
    public static class Hasher
    {
    
        public static string HashPassword(string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
            {
                throw new ArgumentException("La contraseña no puede estar vacía.");
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