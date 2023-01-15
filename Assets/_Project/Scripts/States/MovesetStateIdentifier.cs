using Fusion;

namespace rwby
{
    [System.Serializable]
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