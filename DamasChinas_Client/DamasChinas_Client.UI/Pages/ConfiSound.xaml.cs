using System.Windows;
using System.Windows.Controls;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Diagnostics;

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

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_SoundSettingsError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfiSound.Init - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }

       

        private void OnMusicVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                _pendingVolume = e.NewValue / 100;

                if (_pendingVolume < 0 || _pendingVolume > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(_pendingVolume));
                }

                SoundManager.ApplyVolume(_pendingVolume);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine($"[ConfiSound.OnMusicVolumeChanged - OutOfRange] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_SoundVolumeInvalid"),
                    "warning"
                );
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ConfiSound.OnMusicVolumeChanged - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_SoundSettingsError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfiSound.OnMusicVolumeChanged - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }

        

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_pendingVolume < 0 || _pendingVolume > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(_pendingVolume));
                }

                SoundManager.ApplyVolume(_pendingVolume);

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_SoundSettingsUpdated"),
                    "success"
                );
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine($"[ConfiSound.OnConfirmClick - OutOfRange] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_SoundVolumeInvalid"),
                    "warning"
                );
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ConfiSound.OnConfirmClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_SoundSettingsError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfiSound.OnConfirmClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
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
                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                        "warning"
                    );
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ConfiSound.OnBackClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfiSound.OnBackClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }
    }
}
