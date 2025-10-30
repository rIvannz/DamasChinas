using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;

namespace DamasChinas_Client.UI.Utilities
{
	public static class SoundManager
	{
		private static readonly MediaPlayer MusicPlayer = new MediaPlayer();
		private static bool _initialized;

		public static double MusicVolume { get; private set; } = 0.5;
		public static bool IsPlaying { get; private set; }

		/// <summary>
		/// Inicializa y reproduce la música global del juego.
		/// </summary>
		public static void Initialize()
		{
			if (_initialized)
			{
				return;
			}

			try
			{
				const string musicPath = @"C:\\Projects\\DamasChinas_Client\\Assets\\Sounds\\background_music.mp3";

				if (!File.Exists(musicPath))
				{
					Debug.WriteLine($"[SoundManager] Archivo no encontrado: {musicPath}");
					return;
				}

				var uri = new Uri(musicPath, UriKind.Absolute);
				MusicPlayer.Open(uri);
				MusicPlayer.Volume = MusicVolume;

				MusicPlayer.MediaEnded += (sender, args) =>
				{
					MusicPlayer.Position = TimeSpan.Zero;
					MusicPlayer.Play();
				};

				MusicPlayer.Play();
				IsPlaying = true;
				_initialized = true;
				Debug.WriteLine("[SoundManager] Música iniciada correctamente.");
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"[SoundManager] Error al iniciar música: {ex.Message}");
			}
		}

		/// <summary>
		/// Aplica el nuevo volumen.
		/// </summary>
		public static void ApplyVolume(double newVolume)
		{
			MusicVolume = newVolume;
			MusicPlayer.Volume = MusicVolume;
		}

		public static void TogglePlayPause()
		{
			if (IsPlaying)
			{
				MusicPlayer.Pause();
				IsPlaying = false;
			}
			else
			{
				MusicPlayer.Play();
				IsPlaying = true;
			}
		}
	}
}
