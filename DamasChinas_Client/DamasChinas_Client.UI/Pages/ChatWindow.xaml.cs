using DamasChinas_Client.UI.ChatServiceProxy;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
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

        private IChatService _client;

        private DispatcherTimer _refreshTimer;

        public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();

        public ChatWindow(int miId, string friendUsername)
        {
            InitializeComponent();

            _miIdUsuario = miId;
            _friendUsername = friendUsername;
            _myUsername = ObtenerMiUsername();

            DataContext = this;

            var callback = new ChatCallback(this);
            var context = new InstanceContext(callback);

            var binding = new NetTcpBinding
            {
                Security = { Mode = SecurityMode.None },
                ReceiveTimeout = TimeSpan.MaxValue
            };

            var endpoint = new EndpointAddress("net.tcp://localhost:8755/ChatService/");
            var factory = new DuplexChannelFactory<IChatService>(context, binding, endpoint);
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
            return "prueba"; 
        }

        private async void CargarHistorial()
        {
            try
            {
                Messages.Clear();

                var historial = await Task.Run(() => _client.GetHistoricalMessages(_miIdUsuario, _friendUsername).ToList());

                foreach (var msg in historial) 
                
                    Messages.Add(msg);
                    if (MessagesList.Items.Count > 0)
                        {
                    MessagesList.ScrollIntoView(MessagesList.Items[MessagesList.Items.Count - 1]);
                        }
                }

            catch (Exception ex)
            {
                Console.WriteLine("Error al cargar historial: " + ex.Message);
            }
        }

        private void OnSendClick(object sender, RoutedEventArgs e)
        {
            var texto = InputMessage.Text.Trim();
            if (string.IsNullOrEmpty(texto))
            {
                return;
            }
            var mensaje = new Message

            {
                IdUser = _miIdUsuario,
                DestinationUsername = _friendUsername,
                Text = texto,
                SendDate = DateTime.Now
            };

            try
            {
                _client.SendMessage(mensaje);
                Messages.Add(mensaje);
                MessagesList.ScrollIntoView(MessagesList.Items[MessagesList.Items.Count - 1]);
                InputMessage.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al enviar mensaje: " + ex.Message);
            }
        }

        public void RecibirMensaje(Message mensaje)
        {
            if (mensaje.IdUser == _miIdUsuario || mensaje.DestinationUsername == _friendUsername)
            {
                Dispatcher.Invoke(() =>
                {
                    Messages.Add(mensaje);
                    MessagesList.ScrollIntoView(MessagesList.Items[MessagesList.Items.Count - 1]);
                });
            }
        }
    }

    public class ChatCallback : IChatServiceCallback
    {
        private readonly ChatWindow _chatWindow;
        public ChatCallback(ChatWindow chatWindow) => _chatWindow = chatWindow;

        public void Receivemessage(Message mensaje) => _chatWindow.RecibirMensaje(mensaje);
    }
}
