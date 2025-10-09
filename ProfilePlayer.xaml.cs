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


namespace DamasChinas_Client.Pages
{
    public partial class ProfilePlayer : Page
    {
        public ProfilePlayer()
        {
            InitializeComponent();
        }

        // ===== Botón Back =====
        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        // ===== Botón Change Data =====
        private void OnChangeDataClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChangeData());
        }

        // ===== Botón Sonido =====
        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ConfiSound());
        }

        // ===== Botón Idioma =====
        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SelectLanguage());
        }
    }
}


