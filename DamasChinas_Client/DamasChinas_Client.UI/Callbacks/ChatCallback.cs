using DamasChinas_Client.UI.MensajeriaService;
using System;
using System.Windows;

namespace DamasChinas_Client.UI.Callbacks
{
    public class ChatCallback : IChatServiceCallback
    {
        private readonly Action<Message> _onMessageReceived;

        public ChatCallback(Action<Message> onMessageReceived)
        {
            _onMessageReceived = onMessageReceived;
        }

        public void ReceiveMessage(Message message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _onMessageReceived?.Invoke(message);
            });
        }
    }
}
