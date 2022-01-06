using Fusion;

namespace rwby
{
    public struct LobbySettings : INetworkStruct
    {
        public byte teams;
        [Networked, Capacity(30)] public string gamemodeReference { get => default; set { } }
    }
}
