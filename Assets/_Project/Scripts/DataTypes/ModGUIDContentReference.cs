using System;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    [System.Serializable]
    public struct ModGUIDContentReference : IEquatable<ModGUIDContentReference>
    {
        public ContentGUID modGUID;
        public int contentType;
        public int contentIdx;

        public ModGUIDContentReference(ContentGUID modGUID, int contentType, int contentIdx)
        {
            this.modGUID = modGUID;
            this.contentType = contentType;
            this.contentIdx = contentIdx;
        }

        public ModGUIDContentReference(byte[] modGUID, int contentType, int contentIdx)
        {
            this.modGUID = new ContentGUID(modGUID);
            this.contentType = contentType;
            this.contentIdx = contentIdx;
        }
        
        public bool IsValid()
        {
            if (contentType == (int)ContentType.NONE) return false;
            return true;
        }

        public override string ToString()
        {
            return $"{modGUID.ToString()}:{contentType}:{contentIdx}";
        }

        public bool Equals(ModGUIDContentReference other)
        {
            return contentType == other.contentType && modGUID.Equals(other.modGUID) && contentIdx.Equals(other.contentIdx);
        }

        public override bool Equals(object obj)
        {
            return obj is ModGUIDContentReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(modGUID, contentType, contentIdx);
        }
        
        public static bool operator ==(ModGUIDContentReference x, ModGUIDContentReference y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(ModGUIDContentReference x, ModGUIDContentReference y)
        {
            return !(x == y);
        }
        
        public static implicit operator NetworkModObjectGUIDReference(ModGUIDContentReference nmo) =>
            new NetworkModObjectGUIDReference(nmo.modGUID, nmo.contentType, nmo.contentIdx);
    }
}