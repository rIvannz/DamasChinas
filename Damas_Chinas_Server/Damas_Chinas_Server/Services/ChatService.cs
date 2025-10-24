using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
namespace Damas_Chinas_Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ChatService : IChatService
    {
 
        private static Dictionary<string, IChatCallback> clientes = new Dictionary<string, IChatCallback>();

     
        private ChatRepository _repo = new ChatRepository();

    
        public void RegistrarCliente(string username)
        {
            var callback = OperationContext.Current.GetCallbackChannel<IChatCallback>();
            if (!clientes.ContainsKey(username))
                clientes[username] = callback;
        }

       
        public void SendMessage(Message message)
        {
            
            int idUserSender = message.IdUser;
            int idUserRecipient = _repo.GetIdByUsername(message.DestinationUsername);

            _repo.SaveMessage(idUserSender, idUserRecipient, message.Text);

            if (clientes.ContainsKey(message.DestinationUsername))
            {
                try
                {
                    clientes[message.DestinationUsername].Receivemessage(message);
                }
                catch
                {
                    clientes.Remove(message.DestinationUsername);
                }
            }
        }

        public Message[] GetHistoricalMessages(int idUsuario, string usernameDestino)
        {
            return _repo.GetChatByUsername(idUsuario, usernameDestino).ToArray();
        }
    }
}
