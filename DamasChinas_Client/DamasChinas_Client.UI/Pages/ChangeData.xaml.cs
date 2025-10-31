using System;
using System.Windows;
using System.Windows.Controls;
using DamasChinas_Client.UI.AccountManagerServiceProxy;
using DamasChinas_Client.UI.Utilities;



namespace DamasChinas_Client.UI.Pages
{
    /// <summary>
    /// Interaction logic for ChangeData.xaml
    /// Allows the user to update their username and password.
    /// </summary>
    public partial class ChangeData : Page
    {
        private PublicProfile _profile;

        public ChangeData()
        {
            InitializeComponent();
        }

        public ChangeData(PublicProfile profile)
            : this()
        {
            _profile = profile;
            LoadProfileData();
        }

        // -------------------------------
        // 🔹 General navigation
        // -------------------------------

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void OnSendCodeClick(object sender, RoutedEventArgs e)
        {
            TryExecuteAction(() =>
            {
                MessageHelper.ShowSuccess("Code sent successfully.");
            }, "Error sending code");
        }

        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            TryExecuteAction(() => NavigationService?.Navigate(new ConfiSound()), "Error opening sound settings");
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            TryExecuteAction(() => NavigationService?.Navigate(new SelectLanguage()), "Error opening language settings");
        }

        // -------------------------------
        // 🔹 Username change
        // -------------------------------

        private void OnSaveUsernameClick(object sender, RoutedEventArgs e)
        {
            TryExecuteAction(() =>
            {
                if (!ValidateUsernameInput())
                    return;

                ChangeUsername(txtUsername.Text.Trim());
            }, "Error changing username");
        }

        private bool ValidateUsernameInput()
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageHelper.ShowWarning("Username cannot be empty.");
                return false;
            }
            return true;
        }

        private void ChangeUsername(string newUsername)
        {
            using (var client = new AccountManagerClient())
            {
                var result = client.ChangeUsername(_profile.Username, newUsername);

                if (result.Success)
                {
                    UpdateUsernameState(newUsername);
                    string message = MessageTranslator.GetLocalizedMessage(result.Code);
                    MessageHelper.ShowSuccess(message);
                    NavigationService?.GoBack();
                }
                else
                {
                    string message = MessageTranslator.GetLocalizedMessage(result.Code);
                    MessageHelper.ShowWarning(message, "Warning");
                }
            }
        }

        private void UpdateUsernameState(string newUsername)
        {
            _profile.Username = newUsername;
            txtCurrentUsername.Text = _profile.Username;

            if (ClientSession.IsLoggedIn)
            {
                ClientSession.CurrentProfile.Username = newUsername;
            }
        }

        // -------------------------------
        // 🔹 Password change
        // -------------------------------

        private void OnSavePasswordClick(object sender, RoutedEventArgs e)
        {
            TryExecuteAction(() =>
            {
                if (!ValidatePasswordInputs())
                    return;

                if (!ValidatePasswordStrength(txtPassword.Password))
                    return;

                string hashedPassword = Hasher.HashPassword(txtPassword.Password.Trim());
                ChangePassword(_profile.Username, hashedPassword);
            }, "Error changing password");
        }

        private bool ValidatePasswordInputs()
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Password) ||
                string.IsNullOrWhiteSpace(txtConfirmPassword.Password))
            {
                MessageHelper.ShowWarning("Please fill in all fields.");
                return false;
            }

            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                MessageHelper.ShowWarning("Passwords do not match.");
                return false;
            }

            return true;
        }

        private bool ValidatePasswordStrength(string password)
        {
            try
            {
                Validator.ValidatePassword(password);
                MessageHelper.ShowInfo("Password is valid.");
                return true;
            }
            catch (Exception ex)
            {
                MessageHelper.ShowWarning($"Invalid password: {ex.Message}");
                return false;
            }
        }

        private void ChangePassword(string username, string hashedPassword)
        {
            using (var client = new AccountManagerClient())
            {
                var result = client.ChangePassword(username, hashedPassword);

                if (result.Success)
                {
                    string message = MessageTranslator.GetLocalizedMessage(result.Code);
                    MessageHelper.ShowSuccess(message);
                    ClearPasswordInputs();
                }
                else
                {
                    string message = MessageTranslator.GetLocalizedMessage(result.Code);
                    MessageHelper.ShowWarning(message, "Warning");
                }
            }
        }

        private void ClearPasswordInputs()
        {
            txtPassword.Password = string.Empty;
            txtConfirmPassword.Password = string.Empty;
        }

        // -------------------------------
        // 🔹 Utilities
        // -------------------------------

        private void LoadProfileData()
        {
            try
            {
                if (_profile != null)
                {
                    txtFirstName.Text = _profile.Name;
                    txtLastName.Text = _profile.LastName;
                    txtEmail.Text = _profile.Email;
                    txtCurrentUsername.Text = _profile.Username;
                }
                else
                {
                    MessageHelper.ShowWarning("User profile not found.", "Profile");
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError("Error loading user data: " + ex.Message);
            }
        }

        private void TryExecuteAction(Action action, string errorMessage)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"{errorMessage}: {ex.Message}");
            }
        }
    }
}
