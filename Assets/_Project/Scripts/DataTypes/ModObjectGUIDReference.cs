using System;
using Fusion;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ModObjectGUIDReference : IEquatable<ModObjectGUIDReference>
    {
        public ContentGUID modGUID;
        public int contentType;
        public ContentGUID contentGUID;

        public ModObjectGUIDReference(ContentGUID modGUID, int contentType, ContentGUID contentGUID)
        {
            this.modGUID = modGUID;
            this.contentType = contentType;
            this.contentGUID = contentGUID;
        }

        public ModObjectGUIDReference(byte[] modGUID, int contentType, byte[] contentGUID)
        {
            this.modGUID = new ContentGUID(modGUID);
            this.contentType = contentType;
            this.contentGUID = new ContentGUID(contentGUID);
        }
        
        public bool IsValid()
        {
            if (contentType == (int)ContentType.NONE) return false;
            return true;
        }

        public override string ToString()
        {
            return $"{modGUID.ToString()}:{contentType}:{contentGUID}";
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