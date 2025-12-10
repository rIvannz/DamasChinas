using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace DamasChinas_Client.UI.Utilities
{
    public class FriendList : INotifyPropertyChanged
    {
        private FriendStatus _status;

        public string Username { get; set; }

        // Archivo del avatar (para BD)
        public string AvatarFile { get; set; }

        // Imagen cargada (para UI)
        public ImageSource AvatarSource { get; set; }

        public FriendStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
