using Fusion;

namespace rwby
{
    [System.Serializable]
    public struct LobbySettings : INetworkStruct
    {
        public byte teams;
        public int maxPlayersPerClient;
        public ModObjectReference gamemodeReference;
    }
}
