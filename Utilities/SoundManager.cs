using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.IO;


namespace DamasChinas_Client.Utilities
{
    public static class SoundManager
    {
        private static readonly MediaPlayer _musicPlayer = new MediaPlayer();
        private static bool _initialized = false;

        public static double MusicVolume { get; private set; } = 0.5;
        public static bool IsPlaying { get; private set; } = false;

        /// <summary>
        /// Inicializa y reproduce la música global del juego.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            try
            {
                // Ruta absoluta del proyecto (válida para desarrollo)
                string musicPath = @"C:\Projects\DamasChinas_Client\Assets\Sounds\background_music.mp3";

                if (!File.Exists(musicPath))
                {
                    System.Diagnostics.Debug.WriteLine($"[SoundManager] Archivo no encontrado: {musicPath}");
                    return;
                }

                Uri uri = new Uri(musicPath, UriKind.Absolute);
                _musicPlayer.Open(uri);
                _musicPlayer.Volume = MusicVolume;

                // Repetir en bucle
                _musicPlayer.MediaEnded += (s, e) =>
                {
                    _musicPlayer.Position = TimeSpan.Zero;
                    _musicPlayer.Play();
                };

                _musicPlayer.Play();
                IsPlaying = true;
                _initialized = true;
                System.Diagnostics.Debug.WriteLine("[SoundManager] Música iniciada correctamente.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SoundManager] Error al iniciar música: {ex.Message}");
            }
        }

        /// <summary>
        /// Aplica el nuevo volumen.
        /// </summary>
        public static void ApplyVolume(double newVolume)
        {
            MusicVolume = newVolume;
            _musicPlayer.Volume = MusicVolume;
        }

        public static void TogglePlayPause()
        {
            if (IsPlaying)
            {
                _musicPlayer.Pause();
                IsPlaying = false;
            }
            else
            {
                _musicPlayer.Play();
                IsPlaying = true;
            }
        }
    }
}
