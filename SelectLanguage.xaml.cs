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
using DamasChinas_Client.Utilities;



namespace DamasChinas_Client
{
    /// <summary>
    /// Interaction logic for SelectLanguage.xaml.
    /// Handles language selection and application.
    /// </summary>
    public partial class SelectLanguage : Page
    {
        public SelectLanguage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles Apply button click.
        /// Uses the LanguageManager to apply the selected language dynamically.
        /// </summary>
        private void OnApplyClick(object sender, RoutedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string languageCode = selectedItem.Tag.ToString();
                LanguageManager.ChangeLanguage(languageCode);
            }
            else
            {
                MessageBox.Show("Selecciona un idioma antes de aplicar.",
                                "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Handles the Back icon click.
        /// Returns to the previous page if possible.
        /// </summary>
        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
            else
                MessageBox.Show("No hay una página anterior para regresar.",
                                "Damas Chinas", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

