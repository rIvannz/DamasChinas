using DamasChinas_Client.Pages;
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


namespace DamasChinas_Client.UI.Pages
{
    public partial class ChangeData : Page
    {
        public ChangeData()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Regresa a la ventana del perfil del jugador.
        /// </summary>
        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ProfilePlayer());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al regresar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Guarda los cambios realizados por el usuario (pendiente de implementación real).
        /// </summary>
        private void OnSaveChangesClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Cambios guardados correctamente.", "Confirmación", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Navega a la ventana de configuración de sonido.
        /// </summary>
        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ConfiSound());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir configuración de sonido: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Navega a la ventana de selección de idioma.
        /// </summary>
        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new SelectLanguage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir configuración de idioma: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}



