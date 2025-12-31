using DamasChinas_Client.UI.Callbacks;
using DamasChinas_Client.UI.LogInServiceProxy;
using DamasChinas_Client.UI.MensajeriaService;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;

namespace DamasChinas_Client.UI.Pages
{
    public partial class ChatWindow : Window
    {
        private readonly string _friendUsername;
        private readonly IChatService _client;

        public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();

        public ChatWindow(string friendUsername)
        {
            InitializeComponent();

            _friendUsername = friendUsername ?? throw new ArgumentNullException(nameof(friendUsername));

            DataContext = this;

            try
            {
                var callback = new ChatCallback(ReceiveMessage);
                var context = new InstanceContext(callback);

                var factory = new DuplexChannelFactory<IChatService>(context, "NetTcpBinding_IChatService");
                _client = factory.CreateChannel();

                _client.RegistrateClient(ClientSession.SafeUsernameNormalized);
                _ = LoadHistoryAsync();
            }
            catch (EndpointNotFoundException ex)
            {
                MessageBox.Show("The chat server is unavailable.\n" + ex.Message, "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException ex)
            {
                MessageBox.Show("A communication error occurred.\n" + ex.Message, "Communication Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show("The request timed out.\n" + ex.Message, "Timeout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show("Chat initialization failed.\n" + ex.Message, "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("Invalid parameter during initialization.\n" + ex.Message, "Parameter Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadHistoryAsync()
        {
            try
            {
                Messages.Clear();
                var history = await Task.Run(() => _client.GetHistoricalMessages(ClientSession.SafeUsername, _friendUsername));

                foreach (var message in history)
                {
                    Messages.Add(message);
                }

                if (MessagesList.Items.Count > 0)
                {
                    MessagesList.ScrollIntoView(MessagesList.Items[MessagesList.Items.Count - 1]);
                }
            }
            catch (CommunicationException ex)
            {
                MessageBox.Show("Could not load message history.\n" + ex.Message, "Communication Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show("Loading history timed out.\n" + ex.Message, "Timeout", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show("The message history could not be processed.\n" + ex.Message, "Processing Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("Invalid data received.\n" + ex.Message, "Data Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnSendClick(object sender, RoutedEventArgs e)
        {
            var text = InputMessage.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var message = new Message
            {
                UsarnameSender = ClientSession.SafeUsername,
                DestinationUsername = _friendUsername,
                Text = text,
                SendDate = DateTime.Now
            };

            try
            {
                _client.SendMessage(message);
                Messages.Add(message);
                MessagesList.ScrollIntoView(MessagesList.Items[MessagesList.Items.Count - 1]);
                InputMessage.Clear();
            }
            catch (CommunicationException ex)
            {
                MessageBox.Show("Failed to send the message.\n" + ex.Message, "Communication Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show("Sending the message timed out.\n" + ex.Message, "Timeout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show("The chat connection is no longer valid.\n" + ex.Message, "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("The message is invalid.\n" + ex.Message, "Message Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ReceiveMessage(Message message)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                Messages.Add(message);
                return;
            }

            dispatcher.BeginInvoke(new Action(() =>
            {
                Messages.Add(message);

                if (MessagesList.Items.Count > 0)
                {
                    MessagesList.ScrollIntoView(MessagesList.Items[MessagesList.Items.Count - 1]);
                }
            }));
        }

    }

}