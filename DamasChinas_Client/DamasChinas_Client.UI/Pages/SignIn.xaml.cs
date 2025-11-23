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



        private async void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            btn.IsEnabled = false;

            SingInServiceClient client = null;
            LoadingWindow loader = null;

            string pendingMessage = null;
            PopupType pendingMessageType = PopupType.Error;
            bool shouldShowPopup = false;

            try
            {
                // VALIDACIONES LOCALES
                if (!ValidateLocalInputs())
                {
                    btn.IsEnabled = true;
                    return;
                }

                // MOSTRAR LOADER
                loader = ShowLoader();

                client = new SingInServiceClient();
                var userDto = GetUserFromInputs();

                // VALIDAR INFORMACIÓN CON EL SERVIDOR
                var isValid = await ValidateWithServerAsync(client, userDto, loader);

                if (!isValid)
                {
                    pendingMessage = MessageTranslator.GetLocalizedMessage("msg_ServerUnavailable");
                    pendingMessageType = PopupType.Error;
                    shouldShowPopup = true;
                    btn.IsEnabled = true;
                    return;
                }

                // EL LOADER FUE CERRADO EN ValidateWithServerAsync
                loader = null;

                // SOLICITAR CÓDIGO DE VERIFICACIÓN
                if (!await RequestVerificationCodeAsync(client, userDto))
                {
                    pendingMessage = MessageTranslator.GetLocalizedMessage("msg_CodeSendingError");
                    pendingMessageType = PopupType.Error;
                    shouldShowPopup = true;
                    btn.IsEnabled = true;
                    return;
                }

                // MOSTRAR POPUP PARA INGRESAR EL CÓDIGO
                var codeValue = ShowVerificationCodeWindow();

                if (string.IsNullOrWhiteSpace(codeValue))
                {
                    btn.IsEnabled = true;
                    return;
                }

                // NUEVO LOADER PARA CREACIÓN DE USUARIO
                loader = ShowLoader();

                if (!await CreateUserAsync(client, userDto, codeValue, loader))
                {
                    pendingMessage = MessageTranslator.GetLocalizedMessage("msg_ServerUnavailable");
                    pendingMessageType = PopupType.Error;
                    shouldShowPopup = true;
                    btn.IsEnabled = true;
                    return;
                }

                // ÉXITO TOTAL
                pendingMessage = MessageTranslator.GetLocalizedMessage("msg_AccountCreated");
                pendingMessageType = PopupType.Success;
                shouldShowPopup = true;
            }
            finally
            {
                btn.IsEnabled = true;
                ServiceHelper.SafeClose(client);

                if (loader != null && loader.IsVisible)
                    loader.Close();

                if (shouldShowPopup && !string.IsNullOrEmpty(pendingMessage))
                {
                    MessageHelper.ShowPopup(pendingMessage, pendingMessageType);
                }
            }
        }



        private bool ValidateLocalInputs()
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Password) ||
                string.IsNullOrWhiteSpace(txtConfirmPassword.Password))
            {
                ShowWarning("msg_EmptyCredentials");
                return false;
            }

            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                ShowWarning("msg_PasswordsDontMatch");
                return false;
            }

            if (!ValidatePassword())
            {
                ShowWarning("msg_InvalidPassword");
                return false;
            }

            return true;
        }


        private LoadingWindow ShowLoader()
        {
            var loader = new LoadingWindow
            {
                Owner = Application.Current.MainWindow
            };
            loader.Show();
            return loader;
        }


        private async Task<bool> ValidateWithServerAsync(SingInServiceClient client, UserDto dto, LoadingWindow loader)
        {
            try
            {
                var result = await client.ValidateUserDataAsync(dto);

                await loader.WaitMinimumAsync();
                loader.Close();

                // Aquí NO mostramos popup nunca
                return result?.Success == true;
            }
            catch (EndpointNotFoundException ex)
            {
                Debug.WriteLine($"[SignIn.ValidateWithServerAsync - EndpointNotFound] {ex.Message}");
                return false;
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[SignIn.ValidateWithServerAsync - Communication] {ex.Message}");
                return false;
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"[SignIn.ValidateWithServerAsync - Timeout] {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignIn.ValidateWithServerAsync - Unknown] {ex.Message}");
                return false;
            }
        }




        private string ShowVerificationCodeWindow()
        {
            var popup = new VerificationCodeWindow
            {
                Owner = Application.Current.MainWindow
            };

            return popup.ShowDialog() == true ? popup.CodeValue : null;
        }


        private async Task<bool> RequestVerificationCodeAsync(SingInServiceClient client, UserDto dto)
        {
            var result = await Task.Run(() => client.RequestVerificationCode(dto.Email));

            if (result?.Success != true)
            {
                var message = result != null
                    ? MessageTranslator.GetLocalizedMessage(result.Code)
                    : MessageTranslator.GetLocalizedMessage("msg_CodeSendingError");
                ShowError(message);
                return false;
            }

            MessageHelper.ShowPopup(
                MessageTranslator.GetLocalizedMessage("msg_CodeSentSuccessfully"),
                PopupType.Success
            );

            return true;
        }


        private async Task<bool> CreateUserAsync(SingInServiceClient client, UserDto dto, string code, LoadingWindow loader)
        {
            try
            {
          
                var result = await client.CreateUserAsync(dto, code);

                await loader.WaitMinimumAsync();

                if (result?.Success != true)
                {
                    HandleCodeCreationError(result);
                    return false;
                }

                return true;
            }
            catch (EndpointNotFoundException ex)
            {
                Debug.WriteLine($"[SignIn.CreateUserAsync - EndpointNotFound] {ex.Message}");
                return false;
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[SignIn.CreateUserAsync - Communication] {ex.Message}");
                return false;
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"[SignIn.CreateUserAsync - Timeout] {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignIn.CreateUserAsync - Unknown] {ex.Message}");
                return false;
            }
        }




        private void ShowWarning(string code)
        {
            MessageHelper.ShowPopup(MessageTranslator.GetLocalizedMessage(code), PopupType.Warning);
        }


        private static void ShowError(string msg)
        {
            MessageHelper.ShowPopup(msg, PopupType.Error);
        }


        private void HandleCodeCreationError(OperationResult result)
        {
            switch (result?.TechnicalDetail)
            {
                case "invalid":
                    ShowError(MessageTranslator.GetLocalizedMessage("msg_InvalidVerificationCode"));
                    break;
                case "expired":
                    ShowError(MessageTranslator.GetLocalizedMessage("msg_VerificationCodeExpired"));
                    break;
                case "not_found":
                    ShowError(MessageTranslator.GetLocalizedMessage("msg_VerificationCodeNotFound"));
                    break;
                default:
                    ShowError(MessageTranslator.GetLocalizedMessage("msg_UnknownError"));
                    break;
            }
        }


        private static void ShowSuccessPopup()
        {
            MessageHelper.ShowPopup(
                MessageTranslator.GetLocalizedMessage("msg_AccountCreated"),
                PopupType.Success
            );
        }

        

        private static async void CloseLoaderIfOpen(LoadingWindow loader)
        {
            if (loader != null)
            {
                await loader.WaitMinimumAsync();
                if (loader.IsVisible)
                    loader.Close();
            }
        }




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
                        PopupType.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignIn.OnBackClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    PopupType.Error
                );
            }
        }

       

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
                    PopupType.Error
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
                Debug.WriteLine($"[SignIn.OnLanguageClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    PopupType.Error
                );
            }
        }
    }
}
