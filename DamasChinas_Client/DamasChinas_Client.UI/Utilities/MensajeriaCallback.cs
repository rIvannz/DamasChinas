using System;
using System.Windows;
using DamasChinas_Client.UI.ChatServiceProxy;
using DamasChinas_Client.UI.Pages;

namespace DamasChinas_Client.UI.Utilities
{
    public class MensajeriaCallback : IChatServiceCallback
    {
        private readonly ChatWindow _chatWindow;

        public MensajeriaCallback(ChatWindow chatWindow)
        {
            _chatWindow = chatWindow;
        }

        public void Receivemessage(Message mensaje)
        {
            _chatWindow.Dispatcher.Invoke(() =>
            {
                _chatWindow.Messages.Add(mensaje);
                _chatWindow.MessagesList.ScrollIntoView(_chatWindow.MessagesList.Items[_chatWindow.MessagesList.Items.Count - 1]);
            });
        }
    }
}
