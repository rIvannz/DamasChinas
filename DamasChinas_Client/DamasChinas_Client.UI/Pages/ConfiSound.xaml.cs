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
        private double _pendingMusicVolume;
        private double _pendingEffectsVolume;

        public ConfiSound()
        {
            InitializeComponent();

            try
            {
                _pendingMusicVolume = SoundManager.MusicVolume;
                MusicSlider.Value = _pendingMusicVolume * 100;
                MusicSlider.ValueChanged += OnMusicVolumeChanged;

                _pendingEffectsVolume = SoundManager.EffectsVolume;
                EffectsSlider.Value = _pendingEffectsVolume * 100;
                EffectsSlider.ValueChanged += OnEffectsVolumeChanged;
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
                throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be between 0 and 1.");
            }
        }

        private void OnMusicVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                _pendingMusicVolume = e.NewValue / 100;
                ValidateVolume(_pendingMusicVolume);

                SoundManager.ApplyMusicVolume(_pendingMusicVolume);
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

        private void OnEffectsVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                _pendingEffectsVolume = e.NewValue / 100;
                ValidateVolume(_pendingEffectsVolume);

                SoundManager.ApplyEffectsVolume(_pendingEffectsVolume);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine($"[ConfiSound.OnEffectsVolumeChanged - OutOfRange] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.SoundVolumeInvalid, PopupType.Warning);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ConfiSound.OnEffectsVolumeChanged - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.SoundSettingsError, PopupType.Error);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine($"[ConfiSound.OnEffectsVolumeChanged - ArgumentException] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.SoundSettingsError, PopupType.Error);
            }
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ValidateVolume(_pendingMusicVolume);
                ValidateVolume(_pendingEffectsVolume);

                SoundManager.ApplyMusicVolume(_pendingMusicVolume);
                SoundManager.ApplyEffectsVolume(_pendingEffectsVolume);

                MessageHelper.ShowPopup(MessageKeys.SoundSettingsUpdated, PopupType.Success);

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
                if (NavigationService?.CanGoBack == true)
                {
                    NavigationService.GoBack();
                    return;
                }

                var hostWindow = Window.GetWindow(this);
                if (hostWindow != null && hostWindow.Owner != null)
                {
                    hostWindow.Close();
                    return;
                }

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
