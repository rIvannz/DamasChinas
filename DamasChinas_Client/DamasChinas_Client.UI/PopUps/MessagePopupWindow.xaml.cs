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

        private const string TypeSuccess = "success";
        private const string TypeError = "error";
        private const string TypeWarning = "warning";
        private const string TypeInfo = "info";

        private const string TitleSuccessKey = "title_Success";
        private const string TitleErrorKey = "title_Error";
        private const string TitleWarningKey = "title_Warning";
        private const string TitleInfoKey = "title_Information";

        private const string GlyphSuccess = "✓";
        private const string GlyphError = "✕";
        private const string GlyphWarning = "!";
        private const string GlyphInfo = "i";

        private const byte SuccessR = 0;
        private const byte SuccessG = 160;
        private const byte SuccessB = 60;

        private const byte ErrorR = 200;
        private const byte ErrorG = 30;
        private const byte ErrorB = 30;

        private const byte WarningR = 230;
        private const byte WarningG = 160;
        private const byte WarningB = 0;

        private const byte InfoR = 0;
        private const byte InfoG = 122;
        private const byte InfoB = 204;

        private const int AutoCloseDelayMs = 2500;

        public MessagePopupWindow(string message, string type = TypeInfo, bool autoClose = false)
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
            type = type?.ToLower() ?? TypeInfo;

            string titleKey;

            if (type == TypeSuccess)
            {
                titleKey = TitleSuccessKey;
            }
            else if (type == TypeError)
            {
                titleKey = TitleErrorKey;
            }
            else if (type == TypeWarning)
            {
                titleKey = TitleWarningKey;
            }
            else
            {
                titleKey = TitleInfoKey;
            }

            TitleText.Text = MessageTranslator.GetLocalizedMessage(titleKey);

            switch (type)
            {
                case TypeSuccess:
                    IconCircle.Background = new SolidColorBrush(Color.FromRgb(SuccessR, SuccessG, SuccessB));
                    IconGlyph.Text = GlyphSuccess;
                    break;

                case TypeError:
                    IconCircle.Background = new SolidColorBrush(Color.FromRgb(ErrorR, ErrorG, ErrorB));
                    IconGlyph.Text = GlyphError;
                    break;

                case TypeWarning:
                    IconCircle.Background = new SolidColorBrush(Color.FromRgb(WarningR, WarningG, WarningB));
                    IconGlyph.Text = GlyphWarning;
                    break;

                default:
                    IconCircle.Background = new SolidColorBrush(Color.FromRgb(InfoR, InfoG, InfoB));
                    IconGlyph.Text = GlyphInfo;
                    break;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_autoClose)
            {
                await Task.Delay(AutoCloseDelayMs);
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
