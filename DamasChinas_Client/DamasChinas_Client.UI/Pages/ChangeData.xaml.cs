using DamasChinas_Client.UI.AccountManagerServiceProxy;
using DamasChinas_Client.UI.PopUps;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

namespace DamasChinas_Client.UI.Pages
{
    public partial class ChangeData : Page
    {
        public ChangeData()
        {
            InitializeComponent();
            LoadProfileData();
        }

     
        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NavigationService == null)
                {
                    MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
                    return;
                }

                NavigationService.GoBack();
            }
            catch (InvalidOperationException)
            {
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }


        private void OnSendCodeClick(object sender, RoutedEventArgs e)
        {
            TryExecuteAction(() =>
            {
                string email = ClientSession.CurrentProfile.Email.Trim().ToLower();
                var client = new AccountManagerClient();

                string cultureCode = LanguageManager.CurrentCultureCode; 
                var result = client.RequestPasswordChangeCode(email, cultureCode);


                if (!result.Success)
                {
                    MessageHelper.ShowPopup(MessageKeys.VerificationCodeSendError, PopupType.Error);
                    return;
                }

                MessageHelper.ShowPopup(MessageKeys.CodeSentSuccessfully, PopupType.Success);
            }, MessageKeys.VerificationCodeSendError);
        }


        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            TryExecuteAction(() =>
            {
                NavigationService?.Navigate(new ConfiSound());
            }, MessageKeys.NavigationError);
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            TryExecuteAction(() =>
            {
                NavigationService?.Navigate(new SelectLanguage());
            }, MessageKeys.NavigationError);
        }

        private void OnSaveUsernameClick(object sender, RoutedEventArgs e)
        {
            TryExecuteAction(() =>
            {
                if (!ValidateUsernameInput())
                {
                    return;
                }

                ChangeUsername(txtUsername.Text.Trim());
            }, MessageKeys.UnknownError);
        }

        private bool ValidateUsernameInput()
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageHelper.ShowPopup(MessageKeys.UsernameEmpty, PopupType.Warning);
                return false;
            }

            return true;
        }

        private void ChangeUsername(string newUsername)
        {
            try
            {
                using (var client = new AccountManagerClient())
                {
                    
                    var result = client.ChangeUsername(ClientSession.SafeUsername, newUsername);

                    string message = MessageTranslator.GetLocalizedMessage(result.Code);

                    if (result.Success)
                    {
                        UpdateUsernameState(newUsername);

                        MessageHelper.ShowPopup(message, PopupType.Success);

                        var profilePage = new ProfilePlayer(ClientSession.CurrentProfile);
                        NavigationService?.Navigate(profilePage);
                    }
                    else
                    {
                        MessageHelper.ShowPopup(message, PopupType.Warning);
                    }
                }
            }
            catch (CommunicationException)
            {
                MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
            }
            catch (TimeoutException)
            {
                MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
            }
            catch (InvalidOperationException)
            {
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }

        private void UpdateUsernameState(string newUsername)
        {
            if (ClientSession.IsLoggedIn)
            {
                ClientSession.CurrentProfile.Username = newUsername;
            }

            txtCurrentUsername.Text = newUsername;

            try
            {
                ClientSession.ResetAllConnections();
            }
            catch
            {
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Warning);
            }
        }



        private void OnSavePasswordClick(object sender, RoutedEventArgs e)
        {
            TryExecuteAction(() =>
            {
                string email = ClientSession.CurrentProfile.Email.Trim().ToLower();
                string code = txtVerificationCode.Text.Trim();
                string newPass = txtPassword.Password.Trim();
                string confirmPass = txtConfirmPassword.Password.Trim();

                if (string.IsNullOrWhiteSpace(code))
                {
                    MessageHelper.ShowPopup(MessageKeys.EmptyCredentials, PopupType.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(newPass) || string.IsNullOrWhiteSpace(confirmPass))
                {
                    MessageHelper.ShowPopup(MessageKeys.EmptyCredentials, PopupType.Warning);
                    return;
                }

                if (newPass != confirmPass)
                {
                    MessageHelper.ShowPopup(MessageKeys.PasswordsDontMatch, PopupType.Warning);
                    return;
                }

                Validator.ValidatePassword(newPass);

                string hashedPassword = Hasher.HashPassword(newPass);

                var client = new AccountManagerClient();
                var result = client.ConfirmPasswordChange(email, code, hashedPassword);

                if (!result.Success)
                {
                    MessageHelper.ShowPopup(MessageKeys.InvalidVerificationCode, PopupType.Warning);
                    return;
                }

                MessageHelper.ShowPopup(MessageKeys.Success, PopupType.Success);
                ClearPasswordInputs();

            }, MessageKeys.UnknownError);
        }


        private void OnSaveSocialUrlClick(object sender, RoutedEventArgs e)
        {
            TryExecuteAction(() =>
            {
                var profile = ClientSession.CurrentProfile;
                if (profile == null)
                {
                    MessageHelper.ShowPopup(MessageKeys.UserProfileNotFound, PopupType.Warning);
                    return;
                }

                string socialUrl = txtSocialUrl.Text.Trim();

                if (string.IsNullOrWhiteSpace(socialUrl))
                {
                    MessageHelper.ShowPopup(MessageKeys.EmptyCredentials, PopupType.Warning);
                    return;
                }

                using (var client = new AccountManagerClient())
                {
                    var result = client.ChangeSocialUrl(profile.IdUser, socialUrl);

                    string message = MessageTranslator.GetLocalizedMessage(result.Code);

                    if (result.Success)
                    {
                        profile.SocialUrl = socialUrl;            
                        ClientSession.CurrentProfile.SocialUrl = socialUrl;

                        MessageHelper.ShowPopup(message, PopupType.Success);
                    }
                    else
                    {
                        MessageHelper.ShowPopup(message, PopupType.Warning);
                    }
                }

            }, MessageKeys.UnknownError);
        }



        private void ClearPasswordInputs()
        {
            txtPassword.Password = string.Empty;
            txtConfirmPassword.Password = string.Empty;
        }


        private void LoadProfileData()
        {
            try
            {
                if (!ClientSession.IsLoggedIn || ClientSession.CurrentProfile == null)
                {
                    MessageHelper.ShowPopup(MessageKeys.UserProfileNotFound, PopupType.Warning);
                    return;
                }

                var profile = ClientSession.CurrentProfile;

                txtFirstName.Text = profile.Name;
                txtLastName.Text = profile.LastName;
                txtEmail.Text = profile.Email;
                txtCurrentUsername.Text = profile.Username;

                if (txtSocialUrl != null)
                {
                    txtSocialUrl.Text = profile.SocialUrl ?? string.Empty;
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ChangeData.LoadProfileData - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }

        private void OnSaveSocialMediaClick(object sender, RoutedEventArgs e)
        {
            TryExecuteAction(() =>
            {
                string url = txtSocialUrl.Text.Trim();

                if (string.IsNullOrWhiteSpace(url))
                {
                    MessageHelper.ShowPopup(MessageKeys.EmptyCredentials, PopupType.Warning);
                    return;
                }

                using (var client = new AccountManagerClient())
                {
                    int idUser = ClientSession.CurrentProfile.IdUser;

                    var result = client.ChangeSocialUrl(idUser, url);

                    string message = MessageTranslator.GetLocalizedMessage(result.Code);

                    if (result.Success)
                    {
                  
                        ClientSession.CurrentProfile.SocialUrl = url;

                        MessageHelper.ShowPopup(message, PopupType.Success);
                    }
                    else
                    {
                        MessageHelper.ShowPopup(message, PopupType.Warning);
                    }
                }

            }, MessageKeys.UnknownError);
        }



        private static void TryExecuteAction(Action action, string errorKey)
        {
            try
            {
                if (action == null)
                {
                    Debug.WriteLine("[ChangeData.TryExecuteAction] action is null.");
                    MessageHelper.ShowPopup(errorKey, PopupType.Error);
                    return;
                }

                action.Invoke();
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ChangeData.TryExecuteAction - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(errorKey, PopupType.Error);
            }
        }
    }
}
