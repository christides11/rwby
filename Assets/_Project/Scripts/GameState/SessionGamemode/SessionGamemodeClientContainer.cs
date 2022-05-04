using Fusion;

namespace rwby
{
    [System.Serializable]
    public struct SessionGamemodeClientContainer : INetworkStruct
    {
        public PlayerRef clientRef;
        [Networked, Capacity(4)] public NetworkLinkedList<SessionGamemodePlayerDefinition> players => default;
    }
}