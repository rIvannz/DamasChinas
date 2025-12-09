using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DamasChinas_Client.UI.RankingServiceProxy;
using DamasChinas_Client.UI.Utilities;
using DamasChinas_Client.UI.PopUps;
using static DamasChinas_Client.UI.Utilities.MessageKeys;

namespace DamasChinas_Client.UI.Pages
{
    public partial class RankingPage : Page
    {
        public RankingPage()
        {
            InitializeComponent();
            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            LoadRanking();
        }

        private void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            LoadRanking();
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RankingPage.OnBackClick] {ex.Message}");
                MessageHelper.ShowPopup(NavigationError, PopupType.Error);
            }
        }

        // ============================================================
        // CARGA DE RANKING
        // ============================================================
        private void LoadRanking()
        {
            RankingServiceClient client = null;

            try
            {
                client = new RankingServiceClient();
                var entries = client.GetTop10Ranking() ?? Array.Empty<RankingEntry>();

                var viewModels = entries
                    .Select((e, index) => new RankingItemViewModel(e, index + 1))
                    .ToList();

                lvRanking.ItemsSource = viewModels;
            }
            catch (FaultException<MessageCode> ex)
            {
                HandleServiceFault(ex.Detail);
            }
            catch (Exception ex) when (
                ex is EndpointNotFoundException ||
                ex is TimeoutException ||
                ex is CommunicationException)
            {
                Debug.WriteLine($"[RankingPage.LoadRanking - Connection] {ex.Message}");
                MessageHelper.ShowPopup(RankingUnavailable, PopupType.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RankingPage.LoadRanking - General] {ex}");
                MessageHelper.ShowPopup(UnknownError, PopupType.Error);
            }
            finally
            {
                CloseClientSafely(client);
            }
        }

        private static void HandleServiceFault(MessageCode code)
        {
            switch (code)
            {
                case MessageCode.RankingUnavailable:
                    MessageHelper.ShowPopup(RankingUnavailable, PopupType.Error);
                    break;

                case MessageCode.ServerUnavailable:
                    MessageHelper.ShowPopup(ServerUnavailable, PopupType.Error);
                    break;

                default:
                    MessageHelper.ShowPopup(UnknownError, PopupType.Error);
                    break;
            }
        }

        private static void CloseClientSafely(RankingServiceClient client)
        {
            if (client == null) return;

            try
            {
                if (client.State == CommunicationState.Opened)
                    client.Close();
                else
                    client.Abort();
            }
            catch
            {
                client.Abort();
            }
        }

        private void OnViewProfileClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.DataContext is RankingItemViewModel vm)
                {
                    NavigationService?.Navigate(
                        new ProfilePublicPage(vm.Username, vm.AvatarFile, vm.MatchesPlayed, vm.Wins, vm.Loses)
                    );
                }
            }
            catch
            {
                MessageHelper.ShowPopup(MessageKeys.ProfileOpenError, PopupType.Error);
            }
        }



        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[RankingPage.OnSoundClick] Not implemented");
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[RankingPage.OnLanguageClick] Not implemented");
        }
    }

    public sealed class RankingItemViewModel
    {
        public RankingItemViewModel(RankingEntry entry, int position)
        {
            Position = position;
            Username = entry.Username;
            AvatarFile = entry.AvatarFile;
            MatchesPlayed = entry.MatchesPlayed;
            Wins = entry.Wins;
            Loses = entry.Loses;
            WinRate = entry.WinRate;
        }

        public int Position { get; }
        public string PositionText => $"#{Position}";

        public string Username { get; }
        public string AvatarFile { get; }

        public int MatchesPlayed { get; }
        public int Wins { get; }
        public int Loses { get; }
        public double WinRate { get; }
    }
}
