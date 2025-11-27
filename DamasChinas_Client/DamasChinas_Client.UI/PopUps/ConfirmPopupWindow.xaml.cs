using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DamasChinas_Client.UI.PopUps
{
    public partial class ConfirmPopupWindow : Window
    {
        public bool Result { get; private set; }

        public ConfirmPopupWindow()
        {
            InitializeComponent();
        }

        public ConfirmPopupWindow(string messageResourceKey) : this()
        {
            if (!string.IsNullOrWhiteSpace(messageResourceKey))
            {
                MessageText.SetResourceReference(TextBlock.TextProperty, messageResourceKey);
            }
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
