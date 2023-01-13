using System;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    [System.Serializable]
    public struct ModContentStringReference : IEquatable<ModContentStringReference>
    {
        public string modGUID;
        public ContentType contentType;
        public string contentGUID;

        public ModContentStringReference(string modGUID, int contentType, string contentGuid)
        {
            this.modGUID = modGUID;
            this.contentType = (ContentType)contentType;
            this.contentGUID = contentGuid;
        }
        
        public ModContentStringReference(string modGUID, ContentType contentType, string contentGuid)
        {
            this.modGUID = modGUID;
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

        public bool Equals(ModContentStringReference other)
        {
            return contentType == other.contentType && modGUID.Equals(other.modGUID) && contentGUID.Equals(other.contentGUID);
        }

        public override bool Equals(object obj)
        {
            return obj is ModContentStringReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(modGUID, contentType, contentGUID);
        }
        
        public static bool operator ==(ModContentStringReference x, ModContentStringReference y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(ModContentStringReference x, ModContentStringReference y)
        {
            return !(x == y);
        }
    }
}