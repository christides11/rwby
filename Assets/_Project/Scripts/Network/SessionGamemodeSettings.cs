using Fusion;

namespace rwby
{
    [System.Serializable]
    public struct SessionGamemodeSettings : INetworkStruct
    {
        public byte teams;
        public int maxPlayersPerClient;
        public ModObjectReference gamemodeReference;
    }
}
