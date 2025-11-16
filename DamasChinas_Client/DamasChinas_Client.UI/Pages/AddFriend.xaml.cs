using DamasChinas_Client.UI.PopUps;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DamasChinas_Client.UI.Pages
{
    public partial class AddFriend : Page
    {
        public AddFriend()
        {
            InitializeComponent();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.GoBack();
            }
            catch (Exception)
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error");
            }
        }

        private void OnSendClick(object sender, RoutedEventArgs e)
        {
            string username = txtFriendUsername.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_EmptyCredentials"),
                    "warning");
                return;
            }

            // TODO: aquí irá la llamada real al servidor para validar usuario.
            bool userFound = !username.Equals("noexiste", StringComparison.OrdinalIgnoreCase);

            var popup = new FriendRequestSent(userFound);

            Window owner = Window.GetWindow(this);
            if (owner != null)
                popup.Owner = owner;

            popup.ShowDialog();

            // Si fue éxito, podemos regresar a Friends
            if (userFound)
            {
                NavigationService?.GoBack();
            }
        }
    }
}
