using Fusion;

namespace rwby
{
    public struct LobbySettings : INetworkStruct
    {
        public byte teams;
        [Networked] public ModObjectReference gamemodeReference { get; set; } // { get => default; set { } }
    }
}
