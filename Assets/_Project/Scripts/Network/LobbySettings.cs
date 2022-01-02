using Fusion;

namespace rwby
{
    public struct LobbySettings : INetworkStruct
    {
        [Networked, Capacity(30)] public string gamemodeReference { get => default; set { } }
    }
}
