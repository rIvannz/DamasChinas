using DamasChinas_Client.UI.AccountManagerServiceProxy;
using DamasChinas_Client.UI.Pages;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DamasChinas_Client.UI.Pages

{
    public partial class ProfilePlayer : Page
    {
        private int _idUsuario;

        public ProfilePlayer()
        {
            InitializeComponent();
        }

        public ProfilePlayer(PublicProfile profile, int idUsuario) : this()
        {
            _idUsuario = idUsuario;

            if (profile != null)
            {
                UsernameTextBlock.Text = profile.Username + _idUsuario;
                FullNameTextBlock.Text = profile.Name + " " + profile.LastName;
                EmailTextBlock.Text = profile.Email;
                // PhoneTextBlock.Text = profile.Telefono; // reutilizadopara redes sociales pendiente pedirle a ivan lo del icono
            }
        }

        // ===== Botón Back =====
        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        // ===== Botón Change Data =====
        private void OnChangeDataClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ChangeData(_idUsuario));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al navegar a cambiar datos: " + ex.Message,
                                "Perfil", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== Botón Sonido =====
        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ConfiSound());
        }

        // ===== Botón Idioma =====
        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SelectLanguage());
        }

        // ===== Método para navegar desde otro lado usando el ID de usuario =====
        public static void NavigateToProfile(Frame frame, int userId)
        {
            try
            {
                var client = new AccountManagerClient();
                var profile = client.GetPublicProfile(userId);

                frame.Navigate(new ProfilePlayer(profile, userId));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener el perfil: " + ex.Message, "Perfil", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
