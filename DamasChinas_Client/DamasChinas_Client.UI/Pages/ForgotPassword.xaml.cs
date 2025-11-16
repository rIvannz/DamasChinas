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
using System.Diagnostics;
using DamasChinas_Client.UI.Utilities;
using DamasChinas_Client.UI.PopUps;



namespace DamasChinas_Client.UI.Pages
{
    public partial class ForgotPassword : Page
    {
        public ForgotPassword()
        {
            InitializeComponent();
            DisablePasswordFields();
        }

        // ================================================================
        //  BOTÓN "Enviar Código"
        // ================================================================
        private void OnSendCodeClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string email = txtEmail.Text.Trim();

          
                Validator.ValidateEmail(email);

             
                var codeWindow = new VerificationCodeWindow
                {
                    Owner = Application.Current.MainWindow
                };

                bool? dialogResult = codeWindow.ShowDialog();

                if (dialogResult == true) // Código correcto (1234)
                {
                    EnablePasswordFields();
                }
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine($"[ForgotPassword.OnSendCodeClick - InvalidEmail] {ex.Message}");
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_InvalidEmail"),
                    "warning"
                );
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ForgotPassword.OnSendCodeClick - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ForgotPassword.OnSendCodeClick - General] {ex.Message}");
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }


        // ================================================================
        //  BOTÓN "Cambiar Contraseña"
        // ================================================================
        private async void OnChangePasswordClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string newPass = txtNewPassword.Password.Trim();
                string confirmPass = txtConfirmPassword.Password.Trim();

                if (string.IsNullOrWhiteSpace(newPass) || string.IsNullOrWhiteSpace(confirmPass))
                {
                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage("msg_EmptyCredentials"),
                        "warning"
                    );
                    return;
                }

                if (newPass != confirmPass)
                {
                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage("msg_PasswordsDontMatch"),
                        "error"
                    );
                    return;
                }

                Validator.ValidatePassword(newPass);

                // Mostrar barra de carga
                var loading = new LoadingWindow
                {
                    Owner = Application.Current.MainWindow
                };

                loading.Show();
                await loading.WaitMinimumAsync();
                loading.Close();

                // TODO: llamada real al servidor para actualizar contraseña

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_PasswordUpdated"),
                    "success"
                );

                // Regresar al login
                NavigationService?.Navigate(new Login());
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine($"[ForgotPassword.OnChangePasswordClick - InvalidPassword] {ex.Message}");
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_InvalidPassword"),
                    "warning"
                );
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ForgotPassword.OnChangePasswordClick - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ForgotPassword.OnChangePasswordClick - General] {ex.Message}");
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }

        // ================================================================
        //  HABILITAR / DESHABILITAR CAMPOS
        // ================================================================
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

        // ================================================================
        //  BOTÓN "Back"
        // ================================================================
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
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
        }

        // ================================================================
        //  SONIDO Y LENGUAJE
        // ================================================================
        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ConfiSound());
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnSoundClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnSoundClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
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
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
        }
    }
}
