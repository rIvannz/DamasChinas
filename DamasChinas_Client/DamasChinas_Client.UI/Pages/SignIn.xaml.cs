using DamasChinas_Client.UI.PopUps;
using DamasChinas_Client.UI.SingInServiceProxy;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
            if (btn == null)
            {
                return;
            }

            btn.IsEnabled = false;

            SingInServiceClient client = null;
            LoadingWindow loader = null;

            try
            {
            
                if (!ValidateLocalInputs())
                {
                    return;
                }

                loader = ShowLoader();
                client = new SingInServiceClient();
                var userDto = GetUserFromInputs();

                bool isValid = await ValidateWithServerAsync(client, userDto, loader);
                loader = null;

                if (!isValid)
                {
                    return;
                }

         
                bool codeRequested = await RequestVerificationCodeAsync(client, userDto);
                if (!codeRequested)
                {
                    return;
                }

              
                string codeValue = ShowVerificationCodeWindow();
                if (string.IsNullOrWhiteSpace(codeValue))
                {
                    return;
                }

                loader = ShowLoader();


                bool userCreated = await CreateUserAsync(client, userDto, codeValue, loader);
                loader = null;

                if (!userCreated)
                {
                    return;
                }


                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_AccountCreated"),
                    PopupType.Success);
            }
            finally
            {
                btn.IsEnabled = true;

                ServiceHelper.SafeClose(client);

                if (loader != null && loader.IsVisible)
                {
                    loader.Close();
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
                ShowWarning(MessageKeys.EmptyCredentials);
                return false;
            }

            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                ShowWarning(MessageKeys.PasswordsDontMatch);
                return false;
            }


            if (txtPassword.Password.Length < 8)
            {
                ShowWarning(MessageKeys.InvalidPasswordLength);
                return false;
            }

      
            if (!ValidatePassword())
            {
                return false;
            }

            return true;
        }

        private bool ValidatePassword()
        {
            try
            {
                Validator.ValidatePassword(txtPassword.Password);
                return true;
            }
            catch (ClientValidationException ex)
            {
                Debug.WriteLine($"[SignIn.ValidatePassword] {ex.Message}");
                ShowWarning(ex.ResourceKey);
                return false;
            }
        }

        private static void ShowWarning(string messageKey)
        {
            string message = MessageTranslator.GetLocalizedMessage(messageKey);
            MessageHelper.ShowPopup(message, PopupType.Warning);
        }

        private static void ShowErrorByKey(string messageKey)
        {
            string message = MessageTranslator.GetLocalizedMessage(messageKey);
            MessageHelper.ShowPopup(message, PopupType.Error);
        }

        private static void ShowError(string message)
        {
            MessageHelper.ShowPopup(message, PopupType.Error);
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

        private static async Task CloseLoaderSafeAsync(LoadingWindow loader)
        {
            if (loader == null)
            {
                return;
            }

            try
            {
                await loader.WaitMinimumAsync();

                if (loader.IsVisible)
                {
                    loader.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignIn.CloseLoaderSafeAsync] {ex.Message}");
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


        private UserDto GetUserFromInputs()
        {
            return new UserDto
            {
                Name = txtFirstName.Text.Trim(),
                LastName = txtLastName.Text.Trim(),
                Email = txtEmail.Text.Trim().ToLower(),
                Username = txtUsername.Text.Trim(),
                Password = Hasher.HashPassword(txtPassword.Password.Trim())
            };
        }


        private static async Task<bool> ValidateWithServerAsync(
            SingInServiceClient client,
            UserDto dto,
            LoadingWindow loader)
        {
            try
            {
                var result = await client.ValidateUserDataAsync(dto);

                await CloseLoaderSafeAsync(loader);

                if (result == null)
                {
                    ShowErrorByKey(MessageKeys.ServerUnavailable);
                    return false;
                }

                if (!result.Success)
                {
                    string message = MessageTranslator.GetLocalizedMessage(result.Code);
                    ShowError(message);
                    return false;
                }

                return true;
            }
            catch (EndpointNotFoundException ex)
            {
                Debug.WriteLine($"[SignIn.ValidateWithServerAsync - EndpointNotFound] {ex.Message}");
                await CloseLoaderSafeAsync(loader);
                ShowErrorByKey(MessageKeys.ServerUnavailable);
                return false;
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[SignIn.ValidateWithServerAsync - Communication] {ex.Message}");
                await CloseLoaderSafeAsync(loader);
                ShowErrorByKey(MessageKeys.ServerUnavailable);
                return false;
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"[SignIn.ValidateWithServerAsync - Timeout] {ex.Message}");
                await CloseLoaderSafeAsync(loader);
                ShowErrorByKey(MessageKeys.NetworkLatency);
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignIn.ValidateWithServerAsync - Unknown] {ex.Message}");
                await CloseLoaderSafeAsync(loader);
                ShowErrorByKey(MessageKeys.UnknownError);
                return false;
            }
        }


        private static async Task<bool> RequestVerificationCodeAsync(
            SingInServiceClient client,
            UserDto dto)
        {
            try
            {
                var result = await Task.Run(() => client.RequestVerificationCode(dto.Email));

                if (result == null)
                {
                    ShowErrorByKey(MessageKeys.ServerUnavailable);
                    return false;
                }

                if (!result.Success)
                {
                    string message = MessageTranslator.GetLocalizedMessage(result.Code);
                    ShowError(message);
                    return false;
                }

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage(MessageKeys.CodeSentSuccessfully),
                    PopupType.Success);

                return true;
            }
            catch (EndpointNotFoundException ex)
            {
                Debug.WriteLine($"[SignIn.RequestVerificationCodeAsync - EndpointNotFound] {ex.Message}");
                ShowErrorByKey(MessageKeys.ServerUnavailable);
                return false;
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[SignIn.RequestVerificationCodeAsync - Communication] {ex.Message}");
                ShowErrorByKey(MessageKeys.ServerUnavailable);
                return false;
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"[SignIn.RequestVerificationCodeAsync - Timeout] {ex.Message}");
                ShowErrorByKey(MessageKeys.NetworkLatency);
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignIn.RequestVerificationCodeAsync - Unknown] {ex.Message}");
                ShowErrorByKey(MessageKeys.UnknownError);
                return false;
            }
        }


        private static async Task<bool> CreateUserAsync(
            SingInServiceClient client,
            UserDto dto,
            string code,
            LoadingWindow loader)
        {
            try
            {
                var result = await client.CreateUserAsync(dto, code);

                await CloseLoaderSafeAsync(loader);

                if (result == null)
                {
                    ShowErrorByKey(MessageKeys.ServerUnavailable);
                    return false;
                }

                if (!result.Success)
                {
                    Debug.WriteLine($"[DEBUG] RESULT: Success={result?.Success}, Code={result?.Code}, Technical={result?.TechnicalDetail}");

                    HandleCodeCreationError(result);
                    return false;
                }

                return true;
            }
            catch (EndpointNotFoundException ex)
            {
                Debug.WriteLine($"[SignIn.CreateUserAsync - EndpointNotFound] {ex.Message}");
                await CloseLoaderSafeAsync(loader);
                ShowErrorByKey(MessageKeys.ServerUnavailable);
                return false;
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[SignIn.CreateUserAsync - Communication] {ex.Message}");
                await CloseLoaderSafeAsync(loader);
                ShowErrorByKey(MessageKeys.ServerUnavailable);
                return false;
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"[SignIn.CreateUserAsync - Timeout] {ex.Message}");
                await CloseLoaderSafeAsync(loader);
                ShowErrorByKey(MessageKeys.NetworkLatency);
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignIn.CreateUserAsync - Unknown] {ex.Message}");
                await CloseLoaderSafeAsync(loader);
                ShowErrorByKey(MessageKeys.UnknownError);
                return false;
            }
        }

        private static void HandleCodeCreationError(OperationResult result)
        {
            switch (result?.TechnicalDetail)
            {
                case "invalid":
                    ShowErrorByKey(MessageKeys.InvalidVerificationCode);
                    break;
                case "expired":
                    ShowErrorByKey(MessageKeys.VerificationCodeExpired);
                    break;
                case "not_found":
                    ShowErrorByKey(MessageKeys.VerificationCodeNotFound);
                    break;
                default:
                    ShowErrorByKey(MessageKeys.UnknownError);
                    break;
            }
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
                        MessageTranslator.GetLocalizedMessage(MessageKeys.NavigationError),
                        PopupType.Warning);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignIn.OnBackClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage(MessageKeys.UnknownError),
                    PopupType.Error);
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
                    MessageTranslator.GetLocalizedMessage(MessageKeys.NavigationError),
                    PopupType.Error);
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
                    MessageTranslator.GetLocalizedMessage(MessageKeys.UnknownError),
                    PopupType.Error);
            }
        }
    }
}
