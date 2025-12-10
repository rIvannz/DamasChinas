using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DamasChinas_Client.UI.Utilities;

namespace DamasChinas_Client.UI.Pages
{
    public partial class ConfiSound : Page
    {
        private double _pendingVolume;

        public ConfiSound()
        {
            InitializeComponent();

            try
            {
                _pendingVolume = SoundManager.MusicVolume;
                MusicSlider.Value = _pendingVolume * 100;

                MusicSlider.ValueChanged += OnMusicVolumeChanged;
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ConfiSound.Init - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.SoundSettingsError, PopupType.Error);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine($"[ConfiSound.Init - ArgumentException] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.SoundSettingsError, PopupType.Error);
            }
        }

        private static void ValidateVolume(double volume)
        {
            if (volume < 0 || volume > 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(volume),
                    "Volume must be between 0 and 1.");
            }
        }

        private void OnMusicVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                _pendingVolume = e.NewValue / 100;

                ValidateVolume(_pendingVolume);

                SoundManager.ApplyVolume(_pendingVolume);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine($"[ConfiSound.OnMusicVolumeChanged - OutOfRange] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.SoundVolumeInvalid, PopupType.Warning);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ConfiSound.OnMusicVolumeChanged - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.SoundSettingsError, PopupType.Error);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine($"[ConfiSound.OnMusicVolumeChanged - ArgumentException] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.SoundSettingsError, PopupType.Error);
            }
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ValidateVolume(_pendingVolume);

                SoundManager.ApplyVolume(_pendingVolume);

                MessageHelper.ShowPopup(MessageKeys.SoundSettingsUpdated, PopupType.Success);

                // Si estamos dentro de una ventana (caso MatchRoom), la cerramos
                var hostWindow = Window.GetWindow(this);
                if (hostWindow != null && hostWindow.Owner != null)
                {
                    hostWindow.Close();
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine($"[ConfiSound.OnConfirmClick - OutOfRange] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.SoundVolumeInvalid, PopupType.Warning);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ConfiSound.OnConfirmClick - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.SoundSettingsError, PopupType.Error);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine($"[ConfiSound.OnConfirmClick - ArgumentException] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.SoundSettingsError, PopupType.Error);
            }
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1) Navegación normal
                if (NavigationService?.CanGoBack == true)
                {
                    NavigationService.GoBack();
                    return;
                }

                // 2) Si estamos incrustados en una ventana modal (MatchRoom), cerrarla
                var hostWindow = Window.GetWindow(this);
                if (hostWindow != null && hostWindow.Owner != null)
                {
                    hostWindow.Close();
                    return;
                }

                // 3) Sin forma de regresar
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Warning);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ConfiSound.OnBackClick - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine($"[ConfiSound.OnBackClick - ArgumentException] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }
    }
}
