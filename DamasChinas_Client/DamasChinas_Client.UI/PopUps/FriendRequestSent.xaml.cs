using DamasChinas_Client.UI.Utilities;

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
    public partial class FriendRequestSent : Window
    {
        public FriendRequestSent(bool success)
        {
            InitializeComponent();
            Configure(success);
        }

        private void Configure(bool success)
        {
            string iconPath;
            string messageKey;

            if (success)
            {
                iconPath = "pack://application:,,,/DamasChinas_Client.UI;component/Assets/Icons/greenCheck.png";
                messageKey = "friendRequestSentOk";
            }
            else
            {
                iconPath = "pack://application:,,,/DamasChinas_Client.UI;component/Assets/Icons/redCross.png";
                messageKey = "msg_FriendUserNotFound";
            }

            StatusIcon.Source = new BitmapImage(new Uri(iconPath, UriKind.Absolute));
            StatusMessage.Text = MessageTranslator.GetLocalizedMessage(messageKey);
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}



