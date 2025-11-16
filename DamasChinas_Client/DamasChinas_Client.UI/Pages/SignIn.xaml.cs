using DamasChinas_Client.UI.PopUps;
using DamasChinas_Client.UI.SingInServiceProxy;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace DamasChinas_Client.UI.Pages
{
    public partial class SignIn : Page
    {
        public SignIn()
        {
            InitializeComponent();
        }

        // ============================================================
        // 🔹 CREATE ACCOUNT CLICK
        // ============================================================

        private async void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            SingInServiceClient client = null;
            Button btn = sender as Button;
            LoadingWindow loadingWindow = null;

            try
            {
                if (btn != null)
                {
                    btn.IsEnabled = false;
                }

                // ============================
                // VALIDACIONES LOCALES
                // ============================
                if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                    string.IsNullOrWhiteSpace(txtLastName.Text) ||
                    string.IsNullOrWhiteSpace(txtEmail.Text) ||
                    string.IsNullOrWhiteSpace(txtUsername.Text) ||
                    string.IsNullOrWhiteSpace(txtPassword.Password) ||
                    string.IsNullOrWhiteSpace(txtConfirmPassword.Password))
                {
                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage("msg_EmptyCredentials"),
                        "warning"
                    );
                    return;
                }

                if (txtPassword.Password != txtConfirmPassword.Password)
                {
                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage("msg_PasswordsDontMatch"),
                        "warning"
                    );
                    return;
                }

                if (!ValidatePassword())
                {
                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage("msg_InvalidPassword"),
                        "warning"
                    );
                    return;
                }

                // ============================
                // MOSTRAR LOADING
                // ============================
                loadingWindow = new LoadingWindow
                {
                    Owner = Application.Current.MainWindow
                };
                loadingWindow.Show();

                // ============================
                // LLAMADA AL SERVICIO
                // ============================
                client = new SingInServiceClient();
                var userDto = GetUserFromInputs();
                var result = await Task.Run(() => client.CreateUser(userDto));

                if (result?.Success != true)
                {
                    // Espera tiempo mínimo antes de cerrar loader
                    await loadingWindow.WaitMinimumAsync();
                    loadingWindow.Close();

                    string msg = MessageTranslator.GetLocalizedMessage(result.Code);
                    MessageHelper.ShowPopup(msg, "error");
                    return;
                }

                // ÉXITO: cerrar loader y abrir ventana de código
                await loadingWindow.WaitMinimumAsync();
                loadingWindow.Close();

                var popup = new VerificationCodeWindow
                {
                    Owner = Application.Current.MainWindow
                };
                popup.ShowDialog();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignIn.OnCreateAccountClick] {ex.Message}");

                if (loadingWindow != null)
                {
                    await loadingWindow.WaitMinimumAsync();

                    if (loadingWindow.IsVisible)
                    {
                        loadingWindow.Close();
                    }
                }

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
            finally
            {
                if (btn != null)
                {
                    btn.IsEnabled = true;
                }

                ServiceHelper.SafeClose(client);
            }
        }




        // ============================================================
        // 🔹 VALIDATION
        // ============================================================

        private bool ValidatePassword()
        {
            try
            {
                Validator.ValidatePassword(txtPassword.Password);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private UserDto GetUserFromInputs()
        {
            return new UserDto
            {
                Name = txtFirstName.Text.Trim(),
                LastName = txtLastName.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Username = txtUsername.Text.Trim(),
                Password = Hasher.HashPassword(txtPassword.Password.Trim())
            };
        }

        private void ClearInputs()
        {
            txtFirstName.Clear();
            txtLastName.Clear();
            txtEmail.Clear();
            txtUsername.Clear();
            txtPassword.Clear();
            txtConfirmPassword.Clear();
        }

        // ============================================================
        // 🔹 BACK
        // ============================================================

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NavigationService?.CanGoBack == true)
                {
                    NavigationService.GoBack();
                }
                else
                {
                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                        "warning"
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignIn.OnBackClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }

        // ============================================================
        // 🔹 SOUND
        // ============================================================

        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ConfiSound());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignIn.OnSoundClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
        }

        // ============================================================
        // 🔹 LANGUAGE
        // ============================================================

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new SelectLanguage());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignIn.OnLanguageClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }
    }
}
