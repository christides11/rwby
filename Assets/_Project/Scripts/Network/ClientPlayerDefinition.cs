using Fusion;

namespace rwby
{
    [System.Serializable]
    public struct ClientPlayerDefinition : INetworkStruct
    {
        public byte team;

        public ModObjectReference characterReference;
        //[Networked, Capacity(15)] public ModObjectReference characterReference { get => default; set { } }
        public NetworkId characterNetID;
    }
}