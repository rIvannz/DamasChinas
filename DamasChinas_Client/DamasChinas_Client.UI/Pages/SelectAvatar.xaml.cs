using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DamasChinas_Client.UI.AccountManagerServiceProxy;
using DamasChinas_Client.UI.Utilities;
using static DamasChinas_Client.UI.Utilities.MessageKeys;

namespace DamasChinas_Client.UI.Pages
{
    public partial class SelectAvatar : Page
    {
        private string _selectedAvatarFile;

        public SelectAvatar()
        {
            InitializeComponent();
            LoadAvatars();
        }

        private void LoadAvatars()
        {
            try
            {
                var avatarFiles = PathProvider.GetAvailableAvatarFiles();

                var paths = avatarFiles
                    .Select(file => PathProvider.GetAvatarPath(file))
                    .ToList();

                AvatarItemsControl.ItemsSource = paths;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SelectAvatar.LoadAvatars] {ex}");
                MessageHelper.ShowPopup(UnknownError, PopupType.Error);
            }
        }

        private void OnAvatarChecked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string fullPath)
            {
                _selectedAvatarFile = System.IO.Path.GetFileName(fullPath);
            }
        }

        private async void OnApplyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_selectedAvatarFile))
                {
                    MessageHelper.ShowPopup(OperationInterrupted, PopupType.Warning);
                    return;
                }

                await UpdateAvatarOnServerAsync(_selectedAvatarFile);

         
                ClientSession.CurrentProfile.AvatarFile = _selectedAvatarFile;

                
                MenuRegisteredPlayer.ForceAvatarRefresh = true;

                MessageHelper.ShowPopup(Success, PopupType.Success);

                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SelectAvatar.OnApplyClick] {ex}");
                MessageHelper.ShowPopup(UnknownError, PopupType.Error);
            }
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NavigationService?.CanGoBack == true)
                    NavigationService.GoBack();
                else
                    MessageHelper.ShowPopup(NavigationError, PopupType.Warning);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SelectAvatar.OnBackClick] {ex}");
                MessageHelper.ShowPopup(UnknownError, PopupType.Error);
            }
        }

        private static async Task UpdateAvatarOnServerAsync(string avatarFile)
        {
            AccountManagerClient client = null;

            try
            {
                client = new AccountManagerClient();

                // ⭐ AHORA USAMOS EL ID DEL USUARIO
                int idUser = ClientSession.CurrentProfile.IdUser;

                await client.ChangeAvatarAsync(idUser, avatarFile);
            }
            finally
            {
                if (client != null)
                {
                    try
                    {
                        if (client.State != System.ServiceModel.CommunicationState.Faulted)
                            client.Close();
                        else
                            client.Abort();
                    }
                    catch
                    {
                        client.Abort();
                    }
                }
            }
        }
    }
}
