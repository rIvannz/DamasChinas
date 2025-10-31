using System;
using System.Text.RegularExpressions;

namespace Damas_Chinas_Server.Utilidades
{
    internal static class Validator
    {

        public static bool ValidateName(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new Exception("El nombre no puede estar vacío.");

            if (nombre.Length < 2 || nombre.Length > 50)
                throw new Exception("El nombre debe tener entre 2 y 50 caracteres.");

            if (!Regex.IsMatch(nombre, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s'-]+$"))
                throw new Exception("El nombre contiene caracteres inválidos.");

            return true;
        }

     
        public static bool ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new Exception("El nombre de usuario no puede estar vacío.");

            if (username.Length < 3 || username.Length > 15)
                throw new Exception("El nombre de usuario debe tener entre 3 y 15 caracteres.");

            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_-]+$"))
                throw new Exception("El nombre de usuario contiene caracteres inválidos.");

            return true;
        }

        public static bool ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new Exception("La contraseña no puede estar vacía.");

            if (password.Length < 8)
                throw new Exception("La contraseña debe tener al menos 8 caracteres.");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                throw new Exception("La contraseña debe tener al menos una letra mayúscula.");

            if (!Regex.IsMatch(password, @"[a-z]"))
                throw new Exception("La contraseña debe tener al menos una letra minúscula.");

            if (!Regex.IsMatch(password, @"[0-9]"))
                throw new Exception("La contraseña debe tener al menos un número.");

            if (!Regex.IsMatch(password, @"[\W_]"))
                throw new Exception("La contraseña debe tener al menos un carácter especial.");

            return true;
        }

  
        public static bool ValidateEmail(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                throw new Exception("El correo no puede estar vacío.");

            if (correo.Length > 100)
                throw new Exception("El correo es demasiado largo.");

            if (!Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new Exception("El correo tiene un formato inválido.");

            return true;
        }
    }
}
