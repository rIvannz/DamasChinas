using System;
using System.Windows;
using DamasChinas_Client.UI.MensajeriaService;
using DamasChinas_Client.UI.Pages;

namespace DamasChinas_Client.UI.Utilities
{
    // Esta clase implementa la interfaz de callback del servicio WCF
    public class MensajeriaCallback : IMensajeriaServiceCallback
    {
        private readonly ChatWindow _chatWindow;

        // Recibe la ventana de chat donde se mostrarán los mensajes
        public MensajeriaCallback(ChatWindow chatWindow)
        {
            _chatWindow = chatWindow;
        }

        // Método que llama el servicio WCF cuando llega un mensaje
        public void RecibirMensaje(Mensaje mensaje)
        {
            // Como este método puede ejecutarse en un hilo distinto, usamos Dispatcher
            _chatWindow.Dispatcher.Invoke(() =>
            {
                _chatWindow.Messages.Add(mensaje);
                _chatWindow.MessagesList.ScrollIntoView(_chatWindow.MessagesList.Items[_chatWindow.MessagesList.Items.Count - 1]);
            });
        }
    }
}
