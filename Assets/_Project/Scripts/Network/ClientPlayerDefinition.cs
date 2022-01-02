using Fusion;

namespace rwby
{
    public struct ClientPlayerDefinition : INetworkStruct
    {
        public sbyte localPlayerID;
        public byte team;
        public NetworkId characterNetID;
    }
}