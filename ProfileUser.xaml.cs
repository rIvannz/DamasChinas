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
    public partial class ProfileUser : Page
    {
        public ProfileUser()
        {
            InitializeComponent();
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ConfiSound());
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SelectLanguage());
        }

        private void OnDeleteFriendClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Friend removed successfully.",
                            "Info",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        private void OnSendMessageClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chat feature coming soon!",
                            "Info",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
    }
}
