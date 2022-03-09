using Fusion;

namespace rwby
{
    [System.Serializable]
    public struct ClientPlayerDefinition : INetworkStruct
    {
        public byte team;

        public ModObjectReference characterReference;
        public NetworkId characterNetID;
    }
}