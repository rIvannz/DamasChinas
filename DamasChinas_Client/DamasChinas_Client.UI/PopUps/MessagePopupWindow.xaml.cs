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
using DamasChinas_Client.UI.Utilities;

namespace DamasChinas_Client.UI.PopUps
{
    public partial class MessagePopupWindow : Window
    {
        private readonly bool _autoClose;

        /// <summary>
        /// Popup universal.
        /// type: success, error, warning, info.
        /// </summary>
        public MessagePopupWindow(string message, string type = "info", bool autoClose = false)
        {
            InitializeComponent();

            MessageText.Text = message;
            _autoClose = autoClose;

            ConfigureVisuals(type);
        }

        private void ConfigureVisuals(string type)
        {
            type = type?.ToLower() ?? "info";

       
            string titleKey;

            if (type == "success")
                titleKey = "title_Success";
            else if (type == "error")
                titleKey = "title_Error";
            else if (type == "warning")
                titleKey = "title_Warning";
            else
                titleKey = "title_Information";


            TitleText.Text = MessageTranslator.GetLocalizedMessage(titleKey);

         

            switch (type)
            {
                case "success":
                    IconCircle.Background = new SolidColorBrush(Color.FromRgb(0, 160, 60));
                    IconGlyph.Text = "✓";
                    break;

                case "error":
                    IconCircle.Background = new SolidColorBrush(Color.FromRgb(200, 30, 30));
                    IconGlyph.Text = "✕";
                    break;

                case "warning":
                    IconCircle.Background = new SolidColorBrush(Color.FromRgb(230, 160, 0));
                    IconGlyph.Text = "!";
                    break;

                default:
                    IconCircle.Background = new SolidColorBrush(Color.FromRgb(0, 122, 204));
                    IconGlyph.Text = "i";
                    break;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_autoClose)
            {
                await Task.Delay(2500);
                Close();
            }
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}


