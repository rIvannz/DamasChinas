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
    /// <summary>
    /// Interaction logic for ProfileUser.xaml.
    /// Represents the profile view of another user (friend).
    /// </summary>
    public partial class ProfileUser : Page
    {
        public ProfileUser()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Maneja el clic en el botón de regreso.
        /// </summary>
        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        /// <summary>
        /// Maneja el clic en el ícono de sonido.
        /// </summary>
        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ConfiSound());
        }

        /// <summary>
        /// Maneja el clic en el ícono de idioma.
        /// </summary>
        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SelectLanguage());
        }

        /// <summary>
        /// Maneja el clic en el botón para eliminar amigo.
        /// </summary>
        private void OnDeleteFriendClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Friend removed successfully.",
                            "Info",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        /// <summary>
        /// Maneja el clic en el botón para enviar mensaje.
        /// </summary>
        private void OnSendMessageClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chat feature coming soon!",
                            "Info",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
    }
}
