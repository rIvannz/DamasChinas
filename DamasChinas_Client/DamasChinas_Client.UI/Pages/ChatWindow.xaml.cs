using DamasChinas_Client.UI.MensajeriaService;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DamasChinas_Client.UI.Pages
{
    public partial class ChatWindow : Window
    {
        private int _miIdUsuario;
        private string _friendUsername;
        private string _myUsername; // username real para registrar en WCF

        private MensajeriaServiceClient _client;

        public ObservableCollection<Mensaje> Messages { get; set; } = new ObservableCollection<Mensaje>();

        // Constructor de dos parámetros, como quieres
        public ChatWindow(int miId, string friendUsername)
        {
            InitializeComponent();

            _miIdUsuario = miId;
            _friendUsername = friendUsername;

            // Aquí obtienes tu username de alguna forma, por ejemplo desde tu sesión
            _myUsername = ObtenerMiUsername();

            DataContext = this;

            // Inicializar cliente WCF con callback
            var callback = new MensajeriaCallback(this);
            var context = new System.ServiceModel.InstanceContext(callback);
            _client = new MensajeriaServiceClient(context);

            // Registrar el cliente con su username real
            _client.RegistrarCliente(_myUsername);

            // Cargar historial
            CargarHistorial();
        }

        private string ObtenerMiUsername()
        {
            // TODO: reemplaza con la forma real de obtener el username del usuario actual
            return "mi_username_real";
        }
        private async void CargarHistorial()
        {
            try
            {
                Messages.Clear();
                var historial = await Task.Run(() => _client.ObtenerHistorialMensajes(_miIdUsuario, _friendUsername));

                MessageBox.Show($"Cantidad de mensajes recibidos: {historial.Count()}");

                foreach (var msg in historial)
                    Messages.Add(msg);

                if (MessagesList.Items.Count > 0)
                    MessagesList.ScrollIntoView(MessagesList.Items[MessagesList.Items.Count - 1]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar historial: " + ex.Message);
            }
        }



        private void OnSendClick(object sender, RoutedEventArgs e)
        {
            var texto = InputMessage.Text.Trim();
            if (string.IsNullOrEmpty(texto)) return;

            var mensaje = new Mensaje
            {
                IdUsuario = _miIdUsuario,
                UsernameDestino = _friendUsername,
                Texto = texto,
                FechaEnvio = DateTime.Now
            };

            try
            {
                _client.EnviarMensaje(mensaje);
                Messages.Add(mensaje);
                MessagesList.ScrollIntoView(MessagesList.Items[MessagesList.Items.Count - 1]);
                InputMessage.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al enviar mensaje: " + ex.Message);
            }
        }

        public void RecibirMensaje(Mensaje mensaje)
        {
            if (mensaje.IdUsuario == _miIdUsuario || mensaje.UsernameDestino == _friendUsername)
            {
                Dispatcher.Invoke(() =>
                {
                    Messages.Add(mensaje);
                    MessagesList.ScrollIntoView(MessagesList.Items[MessagesList.Items.Count - 1]);
                });
            }
        }
    }

    public class MensajeriaCallback : IMensajeriaServiceCallback
    {
        private readonly ChatWindow _chatWindow;
        public MensajeriaCallback(ChatWindow chatWindow) => _chatWindow = chatWindow;

        public void RecibirMensaje(Mensaje mensaje) => _chatWindow.RecibirMensaje(mensaje);
    }
}
