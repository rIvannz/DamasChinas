using Damas_Chinas_Server.Dtos;
using System;
using System.Data.Entity;

namespace Damas_Chinas_Server
{
    public class AccountManager : IAccountManager
    {
        private readonly RepositoryUsers _repository;

        public AccountManager()
        {
            _repository = new RepositoryUsers();
        }

        public PublicProfile GetPublicProfile(int idUser)
        {
            return _repository.GetPublicProfile(idUser);
        }

        public OperationResult ChangeUsername(int idUser, string newUsername)
        {
            try
            {
                bool exito = _repository.ChangeUsername(idUser, newUsername);
                return new OperationResult
                {
                    Succes = exito,
                    Messaje = exito ? "Nombre de usuario actualizado correctamente." : "Error al actualizar el nombre de usuario.",
                    User = null
                };
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Succes = false,
                    Messaje = $"Error al actualizar el nombre de usuario: {ex.Message}",
                    User = null
                };
            }
        }

        public OperationResult ChangePassword(int idUsuario, string nuevaPassword)
        {
            try
            {
                bool OperationResult = _repository.ChangePassword(idUsuario, nuevaPassword);
                return new OperationResult
                {
                    Succes = OperationResult,
                    Messaje = OperationResult ? "Contraseña actualizada correctamente." : "Error al actualizar la contraseña.",
                    User = null
                };
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Succes = false,
                    Messaje = $"Error al actualizar la contraseña: {ex.Message}",
                    User = null
                };
            }
        }
    }
}

