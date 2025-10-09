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
    public partial class ChangeData : Page
    {
        public ChangeData()
        {
            InitializeComponent();
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void OnSaveChangesClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Your changes have been saved successfully.",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ConfiSound());
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SelectLanguage());
        }
    }
}

