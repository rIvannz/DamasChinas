using DamasChinas_Client.UI.Utilities;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

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
            Uri iconUri;
            string messageKey;

            if (success)
            {
             
                iconUri = PathProvider.GetPackUri("Assets/Icons/greenCheck.png");
                messageKey = "friendRequestSentOk";
            }
            else
            {
              
                iconUri = PathProvider.GetPackUri("Assets/Icons/redCross.png");
                messageKey = "msg_FriendUserNotFound";
            }

        
            StatusIcon.Source = new BitmapImage(iconUri);
            StatusMessage.Text = MessageTranslator.GetLocalizedMessage(messageKey);
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}




