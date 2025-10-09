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


namespace DamasChinas_Client
{
    public partial class ConfiSound : Page
    {
        private double _pendingVolume;

        public ConfiSound()
        {
            InitializeComponent();

            // Cargar el volumen actual desde SoundManager
            _pendingVolume = SoundManager.MusicVolume;
            MusicSlider.Value = _pendingVolume * 100;

            // Escuchar cambios dinámicos de volumen
            MusicSlider.ValueChanged += OnMusicVolumeChanged;
        }

        /// <summary>
        /// Se ejecuta cada vez que se mueve el slider.
        /// Permite escuchar los cambios de volumen en tiempo real.
        /// </summary>
        private void OnMusicVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _pendingVolume = e.NewValue / 100;
            SoundManager.ApplyVolume(_pendingVolume);
        }

        /// <summary>
        /// Se ejecuta cuando el usuario presiona Confirmar.
        /// Guarda el valor de volumen aplicado en SoundManager.
        /// </summary>
        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            SoundManager.ApplyVolume(_pendingVolume);

            MessageBox.Show(
                "Sound settings updated successfully.",
                "Sound",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Botón de retroceso: vuelve a la ventana anterior.
        /// </summary>
        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }
    }
}

