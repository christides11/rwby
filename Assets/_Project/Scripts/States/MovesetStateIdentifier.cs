using Fusion;

namespace rwby
{
    public struct MovesetStateIdentifier : INetworkStruct
    {
        public int movesetIdentifier;
        public int stateIdentifier;

        public MovesetStateIdentifier(int movesetID, int stateID)
        {
            movesetIdentifier = movesetID;
            stateIdentifier = stateID;
        }
    }
}