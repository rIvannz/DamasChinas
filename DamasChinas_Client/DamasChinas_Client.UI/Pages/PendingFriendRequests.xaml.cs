using DamasChinas_Client.UI.Utilities;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace DamasChinas_Client.UI.Pages
{
    public partial class PendingFriendRequests : Page
    {
        public ObservableCollection<PendingRequest> Requests { get; } =
            new ObservableCollection<PendingRequest>();

        public PendingFriendRequests()
        {
            InitializeComponent();
            DataContext = this;

            LoadSampleData(); // simulado
        }

        private void LoadSampleData()
        {
            // TODO: cargar solicitudes reales desde el servidor
            Requests.Add(new PendingRequest { Username = "Carlos98" });
            Requests.Add(new PendingRequest { Username = "LuciaDev" });
            Requests.Add(new PendingRequest { Username = "PruebasUser" });
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.GoBack();
            }
            catch
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error");
            }
        }

        private void OnAcceptClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is PendingRequest req)
            {
                // TODO: lógica real
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_FeatureUnavailable"),
                    "info");
            }
        }

        private void OnRejectClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is PendingRequest req)
            {
                // TODO: lógica real para rechazar solicitud

                // Eliminamos la solicitud directamente
                Requests.Remove(req);

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_FeatureUnavailable"),
                    "info");
            }
        }

        public class PendingRequest
        {
            public string Username { get; set; }
        }
    }
}
