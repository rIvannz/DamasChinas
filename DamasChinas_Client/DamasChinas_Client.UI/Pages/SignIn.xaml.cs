using DamasChinas_Client.UI.SingInServiceProxy;
using DamasChinas_Client.UI.Utilities;
using System;
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
            SingInServiceClient client = null;

            try
            {
				if (!ValidatebothPasswords() && ValidatePassword())
				{
					return;
				}



                var userDto = GetUserFromInputs();

                
                client = new SingInServiceClient();
                var result = await Task.Run(() => client.CreateUser(userDto));

                MessageHelper.ShowFromResult(result);

                if (result?.Success == true)
                {
                    ClearInputs();
                }
            }
            catch (ArgumentException ex)
            {
                MessageHelper.ShowWarning(ex.Message);
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Ocurrió un error: {ex.Message}");
            }
            finally
            {
                ServiceHelper.SafeClose(client);
            }
        }

        private bool ValidatebothPasswords()
		{
			if (txtPassword.Password != txtConfirmPassword.Password)
			{
				MessageHelper.ShowWarning("Las contraseñas no coinciden.");
				return false;
			}


			return true;
        }

        private bool ValidatePassword()
        {
            try
            {
                string _password = txtPassword.Password;
                Validator.ValidatePassword(_password);
                return true;
            }
            catch (Exception ex)
            {
                MessageHelper.ShowWarning($"Contraseña inválida: {ex.Message}");
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

		private void OnBackClick(object sender, RoutedEventArgs e)
		{
			if (NavigationService?.CanGoBack == true)
			{
				NavigationService.GoBack();
			}
			else
			{
				MessageHelper.ShowInfo("No previous page found.");
			}
		}

		private void OnSoundClick(object sender, RoutedEventArgs e)
		{
			NavigationService?.Navigate(new ConfiSound());
		}

		private void OnLanguageClick(object sender, RoutedEventArgs e)
		{
			try
			{
				NavigationService?.Navigate(new SelectLanguage());
			}
			catch (Exception ex)
			{
				MessageHelper.ShowError($"Error while opening language settings: {ex.Message}");
			}
		}
	}
}
