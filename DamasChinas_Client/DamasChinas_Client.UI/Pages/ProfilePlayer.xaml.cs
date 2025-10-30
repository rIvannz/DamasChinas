using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DamasChinas_Client.UI.AccountManagerServiceProxy;

namespace DamasChinas_Client.UI.Pages
{
	public partial class ProfilePlayer : Page
	{
		private PublicProfile _profile;

		public ProfilePlayer()
		{
			InitializeComponent();
		}

                public ProfilePlayer(PublicProfile profile)
                {
                        InitializeComponent();

                        _profile = profile ?? throw new ArgumentNullException(nameof(profile));

                        UpdateProfileDisplay();
                }

      

		private void OnBackClick(object sender, RoutedEventArgs e)
		{
			if (NavigationService.CanGoBack)
			{
				NavigationService.GoBack();
			}
		}

		private void OnChangeDataClick(object sender, RoutedEventArgs e)
		{
			try
			{
				NavigationService?.Navigate(new ChangeData(_profile));
			    
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error al navegar a cambiar datos: " + ex.Message, "Perfil", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnSoundClick(object sender, RoutedEventArgs e)
		{
			NavigationService?.Navigate(new ConfiSound());
		}

                private void OnLanguageClick(object sender, RoutedEventArgs e)
                {
                        NavigationService?.Navigate(new SelectLanguage());
                }

                private void UpdateProfileDisplay()
                {
                        if (_profile == null)
                        {
                                return;
                        }

                        UsernameTextBlock.Text = _profile.Username;
                        FullNameTextBlock.Text = $"{_profile.Name} {_profile.LastName}";
                        EmailTextBlock.Text = _profile.Email;
                }
        }
}
