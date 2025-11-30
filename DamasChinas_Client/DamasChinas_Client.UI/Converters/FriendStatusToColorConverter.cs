using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DamasChinas_Client.UI.Utilities;

namespace DamasChinas_Client.UI.Converters
{
    public class FriendStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (FriendStatus)value;

            switch (status)
            {
                case FriendStatus.Online:
                    return new SolidColorBrush(Color.FromRgb(0, 200, 0)); // Verde

                case FriendStatus.InGame:
                    return new SolidColorBrush(Color.FromRgb(200, 0, 0)); // Rojo

                case FriendStatus.Offline:
                default:
                    return new SolidColorBrush(Color.FromRgb(40, 40, 40)); // Negro
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
