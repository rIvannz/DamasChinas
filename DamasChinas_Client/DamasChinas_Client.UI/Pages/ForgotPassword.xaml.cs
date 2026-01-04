using DamasChinas_Client.UI.AccountManagerServiceProxy;
using DamasChinas_Client.UI.PopUps;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DamasChinas_Client.UI.Pages
{
    public partial class ForgotPassword : Page
    {
        private string _verificationCode; 

        public ForgotPassword()
        {
            InitializeComponent();
            DisablePasswordFields();
        }

        private void OnSendCodeClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string email = txtEmail.Text.Trim().ToLower();

                Validator.ValidateEmail(email);

              
                var client = new AccountManagerClient();
                string cultureCode = LanguageManager.CurrentCultureCode;
                var result = client.RequestPasswordChangeCode(email, cultureCode);


                if (!result.Success)
                {
                    MessageHelper.ShowPopup(
                        MessageKeys.UnknownError,
                        PopupType.Error
                    );
                    return;
                }

           
                var codeWindow = new VerificationCodeWindow
                {
                    Owner = Application.Current.MainWindow
                };

                bool? dialogResult = codeWindow.ShowDialog();

                if (dialogResult == true)
                {
                  
                    _verificationCode = codeWindow.CodeValue;

                    EnablePasswordFields();
                }
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine($"[ForgotPassword.OnSendCodeClick - InvalidEmail] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageKeys.InvalidEmail,
                    PopupType.Warning
                );
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ForgotPassword.OnSendCodeClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageKeys.NavigationError,
                    PopupType.Error
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ForgotPassword.OnSendCodeClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageKeys.UnknownError,
                    PopupType.Error
                );
            }
        }

        private async void OnChangePasswordClick(object sender, RoutedEventArgs e)
        {
            try
            {

                string newPass = txtNewPassword.Password.Trim();
                string confirmPass = txtConfirmPassword.Password.Trim();

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
                string Password = Hasher.HashPassword(txtNewPassword.Password.Trim());


                var client = new AccountManagerClient();
                var result = await client.ConfirmPasswordChangeAsync(
                txtEmail.Text.Trim().ToLower(),
                 _verificationCode,
                 Password
                );


                if (!result.Success)
                {
                    MessageHelper.ShowPopup(MessageKeys.InvalidVerificationCode, PopupType.Warning);
                    return;
                }

              
                var loading = new LoadingWindow
                {
                    Owner = Application.Current.MainWindow
                };

                loading.Show();
                await loading.WaitMinimumAsync();
                loading.Close();

               
                MessageHelper.ShowPopup(MessageKeys.Success, PopupType.Success);

                NavigationService?.Navigate(new Login());
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine($"[ForgotPassword.OnChangePasswordClick - InvalidPassword] {ex.Message}");

                MessageHelper.ShowPopup(MessageKeys.InvalidPassword, PopupType.Warning);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ForgotPassword.OnChangePasswordClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Warning);
            }
        }

        private void DisablePasswordFields()
        {
            txtNewPassword.IsEnabled = false;
            txtConfirmPassword.IsEnabled = false;
            btnChangePassword.IsEnabled = false;
        }

        private void EnablePasswordFields()
        {
            txtNewPassword.IsEnabled = true;
            txtConfirmPassword.IsEnabled = true;
            btnChangePassword.IsEnabled = true;
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NavigationService?.CanGoBack == true)
                {
                    NavigationService.GoBack();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ForgotPassword.OnBackClick] {ex.Message}");

                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }

        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ConfiSound());
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ForgotPassword.OnSoundClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ForgotPassword.OnSoundClick - General] {ex.Message}");

                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new SelectLanguage());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ForgotPassword.OnLanguageClick] {ex.Message}");

                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }
    }
}
