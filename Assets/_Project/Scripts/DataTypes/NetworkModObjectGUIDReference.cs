using System;
using Fusion;

namespace rwby
{
    [System.Serializable]
    public struct NetworkModObjectGUIDReference : INetworkStruct, IEquatable<NetworkModObjectGUIDReference>
    {
        public NetworkedContentGUID modGUID;
        public byte contentType;
        public NetworkedContentGUID  contentGUID;

        public NetworkModObjectGUIDReference(ContentGUID modGUID, int contentType, ContentGUID contentGUID)
        {
            this.modGUID = new NetworkedContentGUID(modGUID.guid);
            this.contentType = (byte)contentType;
            this.contentGUID = contentGUID;
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
            new ModObjectGUIDReference(nmo.modGUID.guid.ToArray(), nmo.contentType, nmo.contentGUID.guid.ToArray());
    }
}