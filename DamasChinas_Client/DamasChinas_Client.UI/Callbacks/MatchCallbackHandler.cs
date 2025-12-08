using System;
using System.ServiceModel;
using System.Windows;
using DamasChinas_Client.UI.MatchServiceProxy; // Namespace de tu Referencia de Servicio

namespace DamasChinas_Client.UI.Callbacks
{
    // IMPORTANTE: Evita bloqueos de UI con UseSynchronizationContext = false
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public class MatchCallbackHandler : IMatchServiceCallback
    {
        // Eventos para comunicar a la UI (MatchRoom)
        public event Action<TurnChangeDto> PlayerMoved;
        public event Action<string> MatchEnded;
        public event Action<string> PlayerLeft;
        public event Action<string> ErrorOccurred;

        public void OnPlayerMoved(TurnChangeDto turnInfo)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                PlayerMoved?.Invoke(turnInfo);
            });
        }

        public void OnMatchEnded(string winnerUsername)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MatchEnded?.Invoke(winnerUsername);
            });
        }

        public void OnPlayerLeftMatch(string username)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                PlayerLeft?.Invoke(username);
            });
        }

        public void OnErrorOccurred(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ErrorOccurred?.Invoke(message);
            });
        }
    }
}