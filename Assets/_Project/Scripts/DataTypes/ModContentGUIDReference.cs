using System;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    [System.Serializable]
    public struct ModContentGUIDReference : IEquatable<ModContentGUIDReference>
    {
        public ContentGUID modGUID;
        public int contentType;
        public ContentGUID contentGUID;

        public ModContentGUIDReference(ContentGUID modGUID, int contentType, ContentGUID contentGuid)
        {
            this.modGUID = modGUID;
            this.contentType = contentType;
            this.contentGUID = contentGuid;
        }

        public ModContentGUIDReference(byte[] modGUID, int contentType, ContentGUID contentGuid)
        {
            this.modGUID = new ContentGUID(modGUID);
            this.contentType = contentType;
            this.contentGUID = contentGuid;
        }
        
        public bool IsValid()
        {
            if (contentType == (int)ContentType.NONE) return false;
            return true;
        }

        public override string ToString()
        {
            return $"{modGUID.ToString()}:{contentType.ToString()}:{contentGUID.ToString()}";
        }

        public bool Equals(ModContentGUIDReference other)
        {
            return contentType == other.contentType && modGUID.Equals(other.modGUID) && contentGUID.Equals(other.contentGUID);
        }

        public override bool Equals(object obj)
        {
            return obj is ModContentGUIDReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(modGUID, contentType, contentGUID);
        }
        
        public static bool operator ==(ModContentGUIDReference x, ModContentGUIDReference y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(ModContentGUIDReference x, ModContentGUIDReference y)
        {
            return !(x == y);
        }
    }
}