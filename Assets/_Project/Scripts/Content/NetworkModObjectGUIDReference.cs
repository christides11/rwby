using System;
using Fusion;

namespace rwby
{
    [System.Serializable]
    public struct NetworkModObjectGUIDReference : INetworkStruct, IEquatable<NetworkModObjectGUIDReference>
    {
        public NetworkString<_8> modGUID;
        public int contentType;
        public NetworkString<_8>  contentGUID;

        public NetworkModObjectGUIDReference(string modGUID, int contentType, string contentGUID)
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
        
        public bool Equals(NetworkModObjectGUIDReference other)
        {
            return contentType == other.contentType && modGUID.Equals(other.modGUID) && contentGUID.Equals(other.contentGUID);
        }

        public override bool Equals(object obj)
        {
            return obj is NetworkModObjectGUIDReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(modGUID, contentType, contentGUID);
        }
        
        public static bool operator ==(NetworkModObjectGUIDReference x, NetworkModObjectGUIDReference y)
        {
            return x.Equals(y);
        }
        
        public static bool operator ==(NetworkModObjectGUIDReference x,ModObjectGUIDReference y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(NetworkModObjectGUIDReference x, ModObjectGUIDReference y)
        {
            return !(x == y);
        }

        public static bool operator !=(NetworkModObjectGUIDReference x, NetworkModObjectGUIDReference y)
        {
            return !(x == y);
        }

        public static implicit operator ModObjectGUIDReference(NetworkModObjectGUIDReference nmo) =>
            new ModObjectGUIDReference(nmo.modGUID.Value, nmo.contentType, nmo.contentGUID.Value);
    }
}