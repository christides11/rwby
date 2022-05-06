using System;
using Fusion;
using UnityEngine.Serialization;

namespace rwby
{
    [System.Serializable]
    public struct ModIdentifierTuple : INetworkStruct, IEquatable<ModIdentifierTuple>
    {
        [FormerlySerializedAs("Item1")] public byte source;
        [FormerlySerializedAs("Item2")] public uint identifier;

        public ModIdentifierTuple(byte source, uint identifier)
        {
            this.source = source;
            this.identifier = identifier;
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
            return (Item1: source, Item2: identifier).GetHashCode();
        }
        
        public static bool operator ==(ModIdentifierTuple x, ModIdentifierTuple y) 
        {
            return x.source == y.source && x.identifier == y.identifier;
        }
        public static bool operator !=(ModIdentifierTuple x, ModIdentifierTuple y) 
        {
            return !(x == y);
        }
    }
}