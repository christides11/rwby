using Fusion;

namespace rwby
{
    public struct MovesetStateIdentifier : INetworkStruct
    {
        public int movesetIdentifier;
        public int stateIdentifier;
    }
}