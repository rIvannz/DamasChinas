using DamasChinas_Client.UI.MensajeriaService;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading; 

namespace DamasChinas_Client.UI.Pages
{
    public partial class ChatWindow : Window
    {
        private int _miIdUsuario;
        private string _friendUsername;
        private string _myUsername;

        private IMensajeriaService _client;

        private DispatcherTimer _refreshTimer; 

        public ObservableCollection<Mensaje> Messages { get; set; } = new ObservableCollection<Mensaje>();

        public ChatWindow(int miId, string friendUsername)
        {
            InitializeComponent();

            _miIdUsuario = miId;
            _friendUsername = friendUsername;
            _myUsername = ObtenerMiUsername();

            DataContext = this;

            var callback = new MensajeriaCallback(this);
            var context = new InstanceContext(callback);

            var binding = new NetTcpBinding
            {
                Security = { Mode = SecurityMode.None },
                ReceiveTimeout = TimeSpan.MaxValue
            };

            var endpoint = new EndpointAddress("net.tcp://localhost:8755/MensajeriaService/");
            var factory = new DuplexChannelFactory<IMensajeriaService>(context, binding, endpoint);
            _client = factory.CreateChannel();

      
            _client.RegistrarCliente(_myUsername);

          
            CargarHistorial();

          
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(5);
            _refreshTimer.Tick += (s, e) => CargarHistorial();
            _refreshTimer.Start();
        }

        private string ObtenerMiUsername()
        {
            
            return "mi_username_real";
        }

        private async void CargarHistorial()
        {
            try
            {
                Messages.Clear();
                var historial = await Task.Run(() => _client.ObtenerHistorialMensajes(_miIdUsuario, _friendUsername));

              

                foreach (var msg in historial)
                    Messages.Add(msg);

                if (MessagesList.Items.Count > 0)
                    MessagesList.ScrollIntoView(MessagesList.Items[MessagesList.Items.Count - 1]);
            }
            catch (Exception ex)
            {
         
                Console.WriteLine("Error al cargar historial: " + ex.Message);
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
