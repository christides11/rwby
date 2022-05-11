using System;
using Fusion;

namespace rwby
{
    [System.Serializable]
    public struct NetworkedContentGUID : INetworkStruct, IEquatable<NetworkedContentGUID>
    {
        [Networked, Capacity(8)] public NetworkArray<byte> guid => default;

        public NetworkedContentGUID(byte[] guid)
        {
            var temp = this.guid;
            for (int i = 0; i < this.guid.Length; i++)
            {
                temp.Set(i, guid[i]);
                if (i == guid.Length-1) break;
            }
        }
        
        public override string ToString()
        {
            return ContentGUID.BuildString(guid.ToArray());
        }
        
        public static implicit operator ContentGUID(NetworkedContentGUID ncguid) =>
            new ContentGUID(ncguid.guid.ToArray());

        public bool Equals(NetworkedContentGUID other)
        {
            for (int i = 0; i < guid.Length; i++)
            {
                if (i == other.guid.Length)
                {
                    if (guid[i] == 0) return true;
                    return false;
                }
                if (guid[i] != other.guid[i]) return false;
            }
            return true;
        }
        
        public bool Equals(ContentGUID other)
        {
            for (int i = 0; i < guid.Length; i++)
            {
                if (guid[i] != other.guid[i]) return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is NetworkedContentGUID other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(guid);
        }
        
        public static bool operator ==(NetworkedContentGUID x, NetworkedContentGUID y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(NetworkedContentGUID x, NetworkedContentGUID y)
        {
            return !(x == y);
        }
        
        public static bool operator ==(NetworkedContentGUID x, ContentGUID y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(NetworkedContentGUID x, ContentGUID y)
        {
            return !(x == y);
        }
    }
}