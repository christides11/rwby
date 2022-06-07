using System;
using Fusion;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ModObjectReference : IEquatable<ModObjectReference>
    {
        public ContentGUID modGUID;
        public int contentType;
        public int contentIdx;

        public ModObjectReference(ContentGUID modGUID, int contentType, int contentIdx)
        {
            this.modGUID = modGUID;
            this.contentType = contentType;
            this.contentIdx = contentIdx;
        }

        public ModObjectReference(byte[] modGUID, int contentType, int contentIdx)
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

        public bool Equals(ModObjectReference other)
        {
            return contentType == other.contentType && modGUID.Equals(other.modGUID) && contentIdx == other.contentIdx;
        }

        public override bool Equals(object obj)
        {
            return obj is ModObjectReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(modGUID, contentType, contentIdx);
        }
        
        public static bool operator ==(ModObjectReference x, ModObjectReference y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(ModObjectReference x, ModObjectReference y)
        {
            return !(x == y);
        }
        
        //public static implicit operator NetworkModObjectGUIDReference(ModObjectGUIDReference nmo) =>
        //    new NetworkModObjectGUIDReference(nmo.modGUID, nmo.contentType, nmo.contentGUID);//
    }
}