using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using DamasChinas_Client.UI.Callbacks;
using DamasChinas_Client.UI.LogInServiceProxy;
using DamasChinas_Client.UI.MensajeriaService;

namespace DamasChinas_Client.UI.Pages
{
	public partial class ChatWindow : Window
	{
		private readonly string _friendUsername;
		private readonly string _myUsername;
		private readonly IChatService _client;

		public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();

		public ChatWindow(PublicProfile currentUser, string friendUsername)
		{
			InitializeComponent();

			_friendUsername = friendUsername ?? throw new ArgumentNullException(nameof(friendUsername));
			_myUsername = currentUser?.Username ?? throw new ArgumentNullException(nameof(currentUser));

			DataContext = this;

			try
			{
				var callback = new ChatCallback(ReceiveMessage);
				var context = new InstanceContext(callback);

				var binding = new NetTcpBinding
				{
					Security = { Mode = SecurityMode.None },
					ReceiveTimeout = TimeSpan.MaxValue
				};

				var endpoint = new EndpointAddress("net.tcp://localhost:8755/ChatService/");

				var factory = new DuplexChannelFactory<IChatService>(context, binding, endpoint);
				_client = factory.CreateChannel();

				_client.RegistrateClient(_myUsername);

				_ = LoadHistoryAsync();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error al inicializar el chat: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private async Task LoadHistoryAsync()
		{
			try
			{
				Messages.Clear();
				var history = await Task.Run(() => _client.GetHistoricalMessages(_myUsername, _friendUsername));

				foreach (var message in history)
				{
					Messages.Add(message);
				}

				if (MessagesList.Items.Count > 0)
				{
					MessagesList.ScrollIntoView(MessagesList.Items[MessagesList.Items.Count - 1]);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error al cargar historial: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
				UsarnameSender = _myUsername,
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
			catch (Exception ex)
			{
				MessageBox.Show("Error al enviar mensaje: " + ex.Message);
			}
		}

		public void ReceiveMessage(Message message)
		{
			Messages.Add(message);
			MessagesList.ScrollIntoView(MessagesList.Items[MessagesList.Items.Count - 1]);
		}
	}
}
