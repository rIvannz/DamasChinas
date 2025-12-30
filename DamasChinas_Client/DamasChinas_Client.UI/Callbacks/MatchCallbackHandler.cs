using DamasChinas_Client.UI.MatchServiceProxy;
using DamasChinas_Shared.Contracts.Dtos;
using DamasChinas_Client.UI.Pages;
using System;
using System.ServiceModel;
using System.Windows;

namespace DamasChinas_Client.UI.Callbacks
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public sealed class MatchCallbackHandler : IMatchServiceCallback
    {
        private readonly MatchRoom _page;

        public MatchCallbackHandler(MatchRoom page)
        {
            _page = page ?? throw new ArgumentNullException(nameof(page));
        }

        public void OnPlayerMoved(TurnChangeDto turnInfo)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _page.HandlePlayerMoved(turnInfo);
            }));
        }

        public void OnMatchEnded(string winnerUsername)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _page.HandleMatchEnded(winnerUsername);
            }));
        }

        public void OnPlayerLeftMatch(string username)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _page.HandlePlayerLeft(username);
            }));
        }

        public void OnErrorOccurred(string message)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _page.HandleError(message);
            }));
        }

        public void OnBanStatusUpdated(BanInfoDto banInfo)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _page.HandleBanStatusUpdated(banInfo);
            }));
        }
    }
}
