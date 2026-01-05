using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.Windows.Media;

namespace DamasChinas_Client.UI.Utilities
{
    public static class SoundManager
    {
        private const string MusicFileName = "background_music.mp3";
        private const string MoveEffectFileName = "move.wav";

        private static readonly MediaPlayer MusicPlayer = new MediaPlayer();
        private static readonly List<MediaPlayer> ActiveEffects = new List<MediaPlayer>();

        private static bool _initialized;

        public static double MusicVolume { get; private set; } = 0.5;
        public static double EffectsVolume { get; private set; } = 0.5;

        public static bool IsPlaying { get; private set; }

        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            try
            {
                string musicPath = BuildSoundPath(MusicFileName);
                if (!File.Exists(musicPath))
                {
                    Debug.WriteLine($"[SoundManager.Initialize] Music file not found: {musicPath}");
                    return;
                }

                MusicPlayer.Open(new Uri(musicPath, UriKind.Absolute));
                MusicPlayer.Volume = MusicVolume;

                MusicPlayer.MediaEnded += (sender, args) =>
                {
                    MusicPlayer.Position = TimeSpan.Zero;
                    MusicPlayer.Play();
                };

                MusicPlayer.Play();
                IsPlaying = true;
                _initialized = true;

                Debug.WriteLine("[SoundManager.Initialize] Music started.");
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[SoundManager.Initialize] Error: {ex.Message}");
            }
        }

        public static void ApplyMusicVolume(double newVolume)
        {
            MusicVolume = newVolume;
            MusicPlayer.Volume = MusicVolume;
        }

        public static void ApplyEffectsVolume(double newVolume)
        {
            EffectsVolume = newVolume;
        }

        public static void TogglePlayPause()
        {
            if (IsPlaying)
            {
                MusicPlayer.Pause();
                IsPlaying = false;
                return;
            }

            MusicPlayer.Play();
            IsPlaying = true;
        }

        public static void PlayMoveEffect()
        {
            try
            {
                if (EffectsVolume <= 0)
                {
                    return;
                }

                string effectPath = BuildSoundPath(MoveEffectFileName);
                if (!File.Exists(effectPath))
                {
                    Debug.WriteLine($"[SoundManager.PlayMoveEffect] Effect file not found: {effectPath}");
                    return;
                }

                var player = new MediaPlayer();
                player.Open(new Uri(effectPath, UriKind.Absolute));
                player.Volume = EffectsVolume;

                player.MediaEnded += (s, e) =>
                {
                    try
                    {
                        player.Stop();
                        ActiveEffects.Remove(player);
                    }
                    catch
                    {
                        Debug.WriteLine($"[SoundManager.PlayMoveEffect] Error");

                    }
                };

                ActiveEffects.Add(player);
                player.Play();
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[SoundManager.PlayMoveEffect] Error: {ex.Message}");
            }
        }

        private static string BuildSoundPath(string fileName)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDir, "Assets", "Sounds", fileName);
        }
    }
}
