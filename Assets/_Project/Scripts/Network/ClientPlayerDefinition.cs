using Fusion;

namespace rwby
{
    [System.Serializable]
    public struct ClientPlayerDefinition : INetworkStruct
    {
        public byte team;
        [Networked, Capacity(15)] public string characterReference { get => default; set { } }
        public NetworkId characterNetID;
    }
}