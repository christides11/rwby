using System;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ModObjectSetContentReference : IEquatable<ModObjectSetContentReference>
    {
        [SerializeField] public string modGUID;
        [SerializeField] public string contentGUID;
        
        public ModObjectSetContentReference(string modGUID, string contentGUID)
        {
            this.modGUID = modGUID;
            this.contentGUID = contentGUID;
        }
        
        public override string ToString()
        {
            return $"{modGUID.ToString()}:?:{contentGUID}";
        }

        public bool Equals(ModObjectSetContentReference other)
        {
            return modGUID.Equals(other.modGUID) && contentGUID.Equals(other.contentGUID);
        }

        public override bool Equals(object obj)
        {
            return obj is ModObjectSetContentReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(modGUID, contentGUID);
        }
        
        public static bool operator ==(ModObjectSetContentReference x, ModObjectSetContentReference y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(ModObjectSetContentReference x, ModObjectSetContentReference y)
        {
            return !(x == y);
        }
    }
}