using Fusion;

namespace rwby
{
    public struct IDGroupCollisionInfo : INetworkStruct
    {
        public IDGroupCollisionType collisionType;
        public NetworkId hitIHurtableNetID;
        public int hitByIDGroup;
    }
}