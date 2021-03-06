using Fusion;

namespace rwby
{
    [System.Serializable]
    public struct SessionGamemodePlayerDefinition : INetworkStruct
    {
        public byte team;
        [Networked, Capacity(4)] public NetworkLinkedList<NetworkModObjectGUIDReference> characterReferences => default;

        [Networked, Capacity(4)] public NetworkLinkedList<NetworkId> characterNetworkObjects => default;
    }
}