using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using DamasChinas_Client.UI.Utilities;

namespace DamasChinas_Client.UI.PopUps
{
    public partial class MessagePopupWindow : Window
    {
        private static MessagePopupWindow _openedInstance;
        private static string _lastMessage;
        private static string _lastType;

        private readonly bool _autoClose;

        public bool IsDuplicate { get; private set; }

        public MessagePopupWindow(string message, string type = "info", bool autoClose = false)
        {
            if (IsDuplicateMessage(message, type))
            {
                IsDuplicate = true;
                return;
            }

            RegisterOpenedInstance(this, message, type);

            InitializeComponent();

            MessageText.Text = message;
            _autoClose = autoClose;

            ConfigureVisuals(type);
        }

        private static bool IsDuplicateMessage(string message, string type)
        {
            return _openedInstance != null &&
                   _lastMessage == message &&
                   (_lastType?.ToLower() ?? "") == (type?.ToLower() ?? "");
        }

        private static void RegisterOpenedInstance(
            MessagePopupWindow instance,
            string message,
            string type)
        {
            _openedInstance = instance;
            _lastMessage = message;
            _lastType = type;
        }

        private void ConfigureVisuals(string type)
        {
            type = type?.ToLower() ?? "info";

            string titleKey;

            if (type == "success")
            {
                titleKey = "title_Success";
            }
            else if (type == "error")
            {
                titleKey = "title_Error";
            }
            else if (type == "warning")
            {
                titleKey = "title_Warning";
            }
            else
            {
                titleKey = "title_Information";
            }

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

        protected override void OnClosed(EventArgs e)
        {
            ClearStaticStateIfCurrent(this);
            base.OnClosed(e);
        }

        private static void ClearStaticStateIfCurrent(MessagePopupWindow instance)
        {
            if (_openedInstance == instance)
            {
                _openedInstance = null;
                _lastMessage = null;
                _lastType = null;
            }
        }
    }
}
