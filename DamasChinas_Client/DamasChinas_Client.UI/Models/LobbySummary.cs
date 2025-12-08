namespace DamasChinas_Client.UI.Models
{
    public sealed class LobbySummary
    {
        public int LobbyCode { get; set; }
        public string Code { get; set; }
        public string HostUsername { get; set; }
        public string PlayerCount { get; set; }
        public string IsPrivate { get; set; }
    }
}
