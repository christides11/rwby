using System;
using Fusion;

namespace rwby
{
    [System.Serializable]
    public struct ModIdentifierTuple : INetworkStruct, IEquatable<ModIdentifierTuple>
    {
        public byte Item1;
        public uint Item2;

        public ModIdentifierTuple(byte item1, uint item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public override bool Equals(object obj)
        {
            return obj is ModIdentifierTuple && this == (ModIdentifierTuple)obj;
        }

        public bool Equals(ModIdentifierTuple other)
        {
            return this == (ModIdentifierTuple)other;
        }
        
        public override int GetHashCode()
        {
            return (Item1, Item2).GetHashCode();
        }
        
        public static bool operator ==(ModIdentifierTuple x, ModIdentifierTuple y) 
        {
            return x.Item1 == y.Item1 && x.Item2 == y.Item2;
        }
        public static bool operator !=(ModIdentifierTuple x, ModIdentifierTuple y) 
        {
            return !(x == y);
        }
    }
}