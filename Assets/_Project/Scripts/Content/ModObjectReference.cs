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
            this.modIdentifier = new ModIdentifierTuple(){ Item1 = modIdentifier.Item1, Item2 = modIdentifier.Item2};
            this.objectIdentifier = objectIdentifier;
        }

        public ModObjectReference(ModIdentifierTuple modIdentifier, byte objectIdentifier)
        {
            this.modIdentifier = modIdentifier;
            this.objectIdentifier = objectIdentifier;
        }

        public ModObjectReference(CustomSceneRef sceneRef)
        {
            this.modIdentifier.Item1 = sceneRef.source;
            this.modIdentifier.Item2 = sceneRef.modIdentifier;
            objectIdentifier = sceneRef.sceneIndex;
        }

        public bool IsValid()
        {
            if (modIdentifier.Item1 == 0 || objectIdentifier == 0) return false;
            return true;
        }

        public override string ToString()
        {
            return $"{modIdentifier.Item1}:{modIdentifier.Item2}/{objectIdentifier}";
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