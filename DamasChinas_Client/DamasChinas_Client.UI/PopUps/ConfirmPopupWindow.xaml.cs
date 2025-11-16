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
using System.Windows.Shapes;


namespace DamasChinas_Client.UI.PopUps
{
    public partial class ConfirmPopupWindow : Window
    {
        public bool Result { get; private set; }

        public ConfirmPopupWindow()
        {
            InitializeComponent();
        }

        private void OnYesClick(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        private void OnNoClick(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }
    }

}

