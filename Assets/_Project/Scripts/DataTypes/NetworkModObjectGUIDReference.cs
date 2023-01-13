using System;
using Fusion;
using UnityEngine.Serialization;

namespace rwby
{
    [System.Serializable]
    public struct NetworkModObjectGUIDReference : INetworkStruct, IEquatable<NetworkModObjectGUIDReference>
    {
        public uint modGUID;
        public byte contentType;
        public ushort contentIdx;

        public NetworkModObjectGUIDReference(uint modGUID, int contentType, int contentIdx)
        {
            this.modGUID = modGUID;
            this.contentType = (byte)contentType;
            this.contentIdx = (ushort)contentIdx;
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

        public bool Equals(ModIDContentReference other)
        {
            return contentType == other.contentType && modGUID.Equals(other.modID) && contentIdx.Equals(other.contentIdx);
        }
        
        public bool Equals(NetworkModObjectGUIDReference other)
        {
            return contentType == other.contentType && modGUID.Equals(other.modGUID) && contentIdx.Equals(other.contentIdx);
        }

        public override bool Equals(object obj)
        {
            return obj is NetworkModObjectGUIDReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(modGUID, contentType, contentIdx);
        }
        
        public static bool operator ==(NetworkModObjectGUIDReference x, NetworkModObjectGUIDReference y)
        {
            return x.Equals(y);
        }
        
        public static bool operator ==(NetworkModObjectGUIDReference x,ModIDContentReference y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(NetworkModObjectGUIDReference x, ModIDContentReference y)
        {
            return !(x == y);
        }

        public static bool operator !=(NetworkModObjectGUIDReference x, NetworkModObjectGUIDReference y)
        {
            return !(x == y);
        }

        public static implicit operator ModIDContentReference(NetworkModObjectGUIDReference nmo) =>
            new ModIDContentReference(nmo.modGUID, nmo.contentType, nmo.contentIdx);
    }
}