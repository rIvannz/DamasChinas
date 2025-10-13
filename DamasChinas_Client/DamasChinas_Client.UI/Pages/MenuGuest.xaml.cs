using DamasChinas_Client.UI.Pages;
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
    /// Interaction logic for MenuGuest.xaml.
    /// Represents the main menu available for guest users.
    /// </summary>
    public partial class MenuGuest : Page
    {
        /// <summary>
        /// Initializes the components of the guest menu page.
        /// </summary>
        public MenuGuest()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the click event for the "Join a Party" button.
        /// Navigates the user to the Join Party page or shows a message if not implemented.
        /// </summary>
        private void OnJoinPartyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Feature available only for registered users.",
                                "Guest Access", MessageBoxButton.OK, MessageBoxImage.Information);
                // Example: NavigationService?.Navigate(new JoinPartyPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred while joining a party: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the click event for the "How to Play" button.
        /// Displays tutorial or navigates to instructions page.
        /// </summary>
        private void OnHowToPlayClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Tutorial available soon.",
                                "How to Play", MessageBoxButton.OK, MessageBoxImage.Information);
                // Example: NavigationService?.Navigate(new HowToPlayPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tutorial: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the click event for the "Statistics" icon.
        /// Displays information about the unavailability of guest statistics.
        /// </summary>
        private void OnStatisticsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Guest statistics are not available.",
                                "Statistics", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while accessing statistics: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the click event for the "Sound" icon.
        /// Opens the sound configuration page.
        /// </summary>
        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ConfiSound());
        }

        /// <summary>
        /// Handles the click event for the "Back" icon.
        /// Returns to the Main Window.
        /// </summary>
        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new MainWindow());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error returning to the main menu: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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