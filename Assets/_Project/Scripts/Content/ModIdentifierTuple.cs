using Fusion;

namespace rwby
{
    [System.Serializable]
    public struct ModIdentifierTuple : INetworkStruct
    {
        public byte Item1;
        public uint Item2;

        public ModIdentifierTuple(byte item1, uint item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }
}