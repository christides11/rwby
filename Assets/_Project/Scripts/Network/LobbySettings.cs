using Fusion;

namespace rwby
{
    public struct LobbySettings : INetworkStruct
    {
        public byte teams;
        public int maxPlayersPerClient;
        public ModObjectReference gamemodeReference;
    }
}
