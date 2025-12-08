using System;
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

        // Constructor que acepta la clave del recurso para el mensaje
        public ConfirmPopupWindow(string messageResourceKey) : this()
        {
            if (!string.IsNullOrWhiteSpace(messageResourceKey))
            {
                // Busca el recurso en los diccionarios merged de la aplicación
                // Si falla (porque la key no existe), intenta poner el texto directo
                try
                {
                    MessageText.SetResourceReference(TextBlock.TextProperty, messageResourceKey);
                }
                catch
                {
                    MessageText.Text = messageResourceKey;
                }
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