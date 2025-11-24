using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
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
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfiSound.Init - General] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }


        private void OnMusicVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                _pendingVolume = e.NewValue / 100;

                if (_pendingVolume < 0 || _pendingVolume > 1)
                {
                  
                    throw new ArgumentOutOfRangeException("pendingVolume");
                }

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
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfiSound.OnMusicVolumeChanged - General] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }


        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_pendingVolume < 0 || _pendingVolume > 1)
                {
                    
                    throw new ArgumentOutOfRangeException("pendingVolume");
                }

                SoundManager.ApplyVolume(_pendingVolume);

                MessageHelper.ShowPopup(MessageKeys.SoundSettingsUpdated, PopupType.Success);
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
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfiSound.OnConfirmClick - General] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }


        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NavigationService?.CanGoBack == true)
                {
                    NavigationService.GoBack();
                }
                else
                {
                    MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Warning);
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ConfiSound.OnBackClick - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfiSound.OnBackClick - General] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }
    }
}
