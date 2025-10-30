using System;
using System.Windows;
using System.Windows.Controls;
using DamasChinas_Client.UI.AccountManagerServiceProxy;
using DamasChinas_Client.UI.Utilities;

namespace DamasChinas_Client.UI.Pages
{
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
        // 🔹 Navegación general
        // -------------------------------

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void OnSendCodeClick(object sender, RoutedEventArgs e)
        {
            TryExecuteAction(() =>
            {
                MessageHelper.ShowSuccess("Código enviado correctamente.");
            }, "Error al enviar el código");
        }

        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            TryExecuteAction(() => NavigationService?.Navigate(new ConfiSound()), "Error al abrir configuración de sonido");
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            TryExecuteAction(() => NavigationService?.Navigate(new SelectLanguage()), "Error al abrir configuración de idioma");
        }

        // -------------------------------
        // 🔹 Cambio de usuario
        // -------------------------------

        private void OnSaveUsernameClick(object sender, RoutedEventArgs e)
        {
            TryExecuteAction(() =>
            {
                if (!ValidateUsernameInput())
                    return;

                ChangeUsername(txtUsername.Text.Trim());
            }, "Error al cambiar el nombre de usuario");
        }

        private bool ValidateUsernameInput()
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageHelper.ShowWarning("El nombre de usuario no puede estar vacío.");
                return false;
            }
            return true;
        }

        private void ChangeUsername(string newUsername)
        {
            using (var client = new AccountManagerClient())
            {
                var result = client.ChangeUsername(_profile.Username, newUsername);

                if (result.Succes)
                {
                    UpdateUsernameState(newUsername);
                    MessageHelper.ShowSuccess(result.Messaje);
                    NavigationService?.GoBack();
                }
                else
                {
                    MessageHelper.ShowWarning(result.Messaje, "Aviso");
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
        // 🔹 Cambio de contraseña
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
            }, "Error al cambiar la contraseña");
        }

        private bool ValidatePasswordInputs()
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Password) ||
                string.IsNullOrWhiteSpace(txtConfirmPassword.Password))
            {
                MessageHelper.ShowWarning("Por favor llena todos los campos.");
                return false;
            }

            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                MessageHelper.ShowWarning("Las contraseñas no coinciden.");
                return false;
            }

            return true;
        }

        private bool ValidatePasswordStrength(string password)
        {
            try
            {
                Validator.ValidatePassword(password);
                MessageHelper.ShowInfo("Contraseña válida.");
                return true;
            }
            catch (Exception ex)
            {
                MessageHelper.ShowWarning($"Contraseña inválida: {ex.Message}");
                return false;
            }
        }

        private void ChangePassword(string username, string hashedPassword)
        {
            using (var client = new AccountManagerClient())
            {
                var result = client.ChangePassword(username, hashedPassword);

                if (result.Succes)
                {
                    MessageHelper.ShowSuccess(result.Messaje);
                    ClearPasswordInputs();
                }
                else
                {
                    MessageHelper.ShowWarning(result.Messaje, "Aviso");
                }
            }
        }

        private void ClearPasswordInputs()
        {
            txtPassword.Password = string.Empty;
            txtConfirmPassword.Password = string.Empty;
        }

        // -------------------------------
        // 🔹 Utilidades
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
                    MessageHelper.ShowWarning("No se encontró el perfil del usuario.", "Perfil");
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError("Error al cargar los datos del usuario: " + ex.Message);
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
