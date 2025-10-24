using DamasChinas_Client.UI.FriendServiceProxy;
using DamasChinas_Client.UI.Utilities;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace DamasChinas_Client.UI.Pages
{
    public partial class Friends : Page
    {
        public ObservableCollection<Amigos> FriendList { get; set; } = new ObservableCollection<Amigos>();
        private int _idUser;

      
        public Friends(int idUsuario)
        {
            InitializeComponent();
            DataContext = this;
            _idUser = idUsuario;

            ChargeFriendsToView();
        }

       
        private void ChargeFriendsToView()
        {
            try
            {
                using (var client = new FriendServiceClient())
                {
                    var friendList = client.GetFriends(_idUser);

                    this.FriendList.Clear();
                    foreach (var friend in friendList)
                    {
                        this.FriendList.Add(new Amigos
                        {
                            Username = friend.Username,
                            ConnectionStatus = friend.ConnectionState,
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
                var chatWindow = new ChatWindow(_idUser, amigo.Username);
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
