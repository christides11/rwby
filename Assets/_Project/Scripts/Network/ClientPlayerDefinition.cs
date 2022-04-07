using Fusion;

namespace rwby
{
    [System.Serializable]
    public struct ClientPlayerDefinition : INetworkStruct
    {
        public byte team;
        [Networked, Capacity(4)] public NetworkLinkedList<ModObjectReference> characterReferences => default;
        public NetworkId characterNetID;
    }
}