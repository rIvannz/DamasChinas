using System;
using DamasChinas_Client.UI.LogInServiceProxy;

namespace DamasChinas_Client.UI.Utilities
{
	/// <summary>
	/// Maneja la sesión actual del cliente, incluyendo el perfil del jugador.
	/// Solo puede ser modificada internamente (por el servidor o proceso de login).
	/// </summary>
	public static class ClientSession
	{
		private static PublicProfile _currentProfile;

		/// <summary>
		/// Obtiene el perfil del jugador actualmente autenticado.
		/// </summary>
		public static PublicProfile CurrentProfile
		{
			get
			{
				if (_currentProfile == null)
				{
					throw new InvalidOperationException("No hay una sesión activa. Inicia sesión primero.");
				}

				return _currentProfile;
			}
		}

		/// <summary>
		/// Indica si hay una sesión activa.
		/// </summary>
		public static bool IsLoggedIn => _currentProfile != null;

		/// <summary>
		/// Establece el perfil actual.
		/// Solo debe llamarse desde el servicio de Login o el servidor.
		/// </summary>
		/// <param name="profile">Perfil devuelto por el servidor tras iniciar sesión.</param>
		public static void Initialize(PublicProfile profile)
		{
			_currentProfile = profile ?? throw new ArgumentNullException(nameof(profile));
		}

		/// <summary>
		/// Limpia la sesión actual (por ejemplo, al cerrar sesión).
		/// </summary>
		public static void Clear()
		{
			_currentProfile = null;
		}
	}
}
