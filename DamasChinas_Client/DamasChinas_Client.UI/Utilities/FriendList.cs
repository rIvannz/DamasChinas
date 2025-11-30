using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DamasChinas_Client.UI.Utilities
{
    public class FriendList : INotifyPropertyChanged
    {
        private FriendStatus _status;

        public string Username { get; set; }
        public string Avatar { get; set; }

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
