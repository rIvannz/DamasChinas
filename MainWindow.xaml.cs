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

namespace DamasChinas_Client
{
    /// <summary>
    /// Interaction logic for MainPage.xaml.
    /// </summary>
    public partial class MainWindow : Page
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnGoToLoginClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Login());
        }

        private void OnGoToSignInClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SignIn());
        }

        private void OnPlayAsGuestClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new MenuGuest());
        }

        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ConfiSound());
        }


        /// <summary>
        /// Handles the click event for the "Language" icon.
        /// Navigates to the SelectLanguage page.
        /// </summary>
        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new SelectLanguage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while opening language settings: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}

