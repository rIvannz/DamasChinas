using DamasChinas_Client.UI.AmistadService;
using DamasChinas_Client.UI.Utilities;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace DamasChinas_Client.UI.Pages
{
    public partial class Friends : Page
    {
        public ObservableCollection<Amigos> AmigosList { get; set; } = new ObservableCollection<Amigos>();
        private int _idUsuario;

        // Constructor que recibe el ID del usuario actual
        public Friends(int idUsuario)
        {
            InitializeComponent();
            DataContext = this;
            _idUsuario = idUsuario;

            CargarAmigos();
        }

        // Método que llama al servicio WCF para obtener la lista de amigos
        private void CargarAmigos()
        {
            try
            {
                using (var client = new AmistadServiceClient())
                {
                    var lista = client.ObtenerAmigos(_idUsuario);

                    AmigosList.Clear();
                    foreach (var amigo in lista)
                    {
                        AmigosList.Add(new Amigos
                        {
                            Username = amigo.Username,
                            EnLinea = amigo.EnLinea,
                            Avatar = "pack://application:,,,/DamasChinas_Client.UI;component/Assets/Icons/avatarIcon.png"
                        });
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"No se pudo cargar la lista de amigos: {ex.Message}",
                                "Amigos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void OnViewProfileClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ProfileUser());
        }

        private void OnChatClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is Amigos amigo)
            {
                var chatWindow = new ChatWindow(_idUsuario, amigo.Username);
                chatWindow.Show();
            }
        }




        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ConfiSound());
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SelectLanguage());
        }
    }
}
