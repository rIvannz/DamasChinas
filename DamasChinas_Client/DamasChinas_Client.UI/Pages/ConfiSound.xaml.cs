using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DamasChinas_Client.Utilities;

namespace DamasChinas_Client.UI.Pages
{
    public partial class ConfiSound : Page
    {
        private double _pendingVolume;

        public ConfiSound()
        {
            InitializeComponent();

            // Cargar volumen actual desde SoundManager
            _pendingVolume = SoundManager.MusicVolume;
            MusicSlider.Value = _pendingVolume * 100;

            // Escuchar cambios dinámicos
            MusicSlider.ValueChanged += OnMusicVolumeChanged;
        }

        private void OnMusicVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _pendingVolume = e.NewValue / 100;
            SoundManager.ApplyVolume(_pendingVolume);
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            SoundManager.ApplyVolume(_pendingVolume);
            MessageBox.Show(
                "Sound settings updated successfully.",
                "Sound",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }
    }
}

