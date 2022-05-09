using System;
using Fusion;

namespace rwby
{
    
    [System.Serializable]
    public struct ModObjectReference : INetworkStruct, IEquatable<ModObjectReference>
    {
        public ModIdentifierTuple modIdentifier;
        public byte objectIdentifier;
        
        public ModObjectReference((byte, uint) modIdentifier, byte objectIdentifier)
        {
            this.modIdentifier = new ModIdentifierTuple(){ source = modIdentifier.Item1, identifier = modIdentifier.Item2};
            this.objectIdentifier = objectIdentifier;
        }

        public ModObjectReference(ModIdentifierTuple modIdentifier, byte objectIdentifier)
        {
            this.modIdentifier = modIdentifier;
            this.objectIdentifier = objectIdentifier;
        }

        public bool IsValid()
        {
            if (modIdentifier.source == 0 || objectIdentifier == 0) return false;
            return true;
        }

        public override string ToString()
        {
            return $"{modIdentifier.source}:{modIdentifier.identifier}/{objectIdentifier}";
        }

        public override bool Equals(object obj)
        {
            return obj is ModObjectReference && this == (ModObjectReference)obj;
        }

        public bool Equals(ModObjectReference other)
        {
            return modIdentifier == other.modIdentifier && objectIdentifier == other.objectIdentifier;
        }

        public override int GetHashCode()
        {
            return (modIdentifier, objectIdentifier).GetHashCode();
        }
        
        public static bool operator ==(ModObjectReference x, ModObjectReference y)
        {
            return x.modIdentifier == y.modIdentifier && x.objectIdentifier == y.objectIdentifier;
        }

        public static bool operator !=(ModObjectReference x, ModObjectReference y)
        {
            return !(x == y);
        }
    }
}