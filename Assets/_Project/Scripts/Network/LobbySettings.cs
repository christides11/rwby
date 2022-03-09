using Fusion;

namespace rwby
{
    public struct LobbySettings : INetworkStruct
    {
        public byte teams;
        public ModObjectReference gamemodeReference;
        //[Networked] public ModObjectReference gamemodeReference { get; set; } // { get => default; set { } }
    }
}
