using System;
using Fusion;

namespace rwby
{
    [System.Serializable]
    public struct ModObjectGUIDReference : IEquatable<ModObjectGUIDReference>
    {
        public string modGUID;
        public int contentType;
        public string contentGUID;

        public ModObjectGUIDReference(string modGUID, int contentType, string contentGUID)
        {
            this.modGUID = modGUID;
            this.contentType = contentType;
            this.contentGUID = contentGUID;
        }
        
        public bool IsValid()
        {
            if (contentType == (int)ContentType.NONE) return false;
            return true;
        }

        public override string ToString()
        {
            return $"{modGUID}:{contentType}:{contentGUID}";
        }

        public bool Equals(ModObjectGUIDReference other)
        {
            return contentType == other.contentType && modGUID.Equals(other.modGUID) && contentGUID.Equals(other.contentGUID);
        }

        public override bool Equals(object obj)
        {
            return obj is ModObjectGUIDReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(modGUID, contentType, contentGUID);
        }
        
        public static bool operator ==(ModObjectGUIDReference x, ModObjectGUIDReference y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(ModObjectGUIDReference x, ModObjectGUIDReference y)
        {
            return !(x == y);
        }
        
        public static implicit operator NetworkModObjectGUIDReference(ModObjectGUIDReference nmo) =>
            new NetworkModObjectGUIDReference(nmo.modGUID, nmo.contentType, nmo.contentGUID);
    }
}