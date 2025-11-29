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
                    
                    var result = client.ChangeUsername(ClientSession.safeUsername, newUsername);

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
        }


        private void OnSavePasswordClick(object sender, RoutedEventArgs e)
        {
            TryExecuteAction(() =>
            {
                if (!ValidateVerificationCodeInput())
                {
                    return;
                }

                if (!ValidatePasswordInputs())
                {
                    return;
                }

                if (!ValidatePasswordStrength(txtPassword.Password))
                {
                    return;
                }

                string hashedPassword = Hasher.HashPassword(txtPassword.Password.Trim());

                ChangePassword(hashedPassword);

            }, MessageKeys.UnknownError);
        }

        private bool ValidateVerificationCodeInput()
        {
            string code = txtVerificationCode.Text.Trim();

            if (string.IsNullOrWhiteSpace(code))
            {
                MessageHelper.ShowPopup(MessageKeys.EmptyCredentials, PopupType.Warning);
                return false;
            }

            return true;
        }

        private bool ValidatePasswordInputs()
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Password) ||
                string.IsNullOrWhiteSpace(txtConfirmPassword.Password))
            {
                MessageHelper.ShowPopup(MessageKeys.EmptyCredentials, PopupType.Warning);
                return false;
            }

            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                MessageHelper.ShowPopup(MessageKeys.PasswordsDontMatch, PopupType.Warning);
                return false;
            }

            return true;
        }

        private static bool ValidatePasswordStrength(string password)
        {
            try
            {
                Validator.ValidatePassword(password);
                return true;
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine($"[ChangeData.ValidatePasswordStrength - Argument] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.InvalidPassword, PopupType.Warning);
            }

            return false;
        }

        private void ChangePassword(string hashedPassword)
        {
            try
            {
                using (var client = new AccountManagerClient())
                {
                   
                    var result = client.ChangePassword(ClientSession.safeUsername, hashedPassword);

                    string message = MessageTranslator.GetLocalizedMessage(result.Code);

                    if (result.Success)
                    {
                        MessageHelper.ShowPopup(message, PopupType.Success);
                        ClearPasswordInputs();
                    }
                    else
                    {
                        MessageHelper.ShowPopup(message, PopupType.Warning);
                    }
                }
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[ChangeData.ChangePassword - Communication] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"[ChangeData.ChangePassword - Timeout] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ChangeData.ChangePassword - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
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
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ChangeData.LoadProfileData - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
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
