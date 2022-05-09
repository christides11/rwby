using Fusion;

namespace rwby
{
    public struct NetworkedContentGUID : INetworkStruct
    {
        [Networked, Capacity(16)] public NetworkArray<byte> guid => default;

        public NetworkedContentGUID(byte[] guid)
        {
            for (int i = 0; i < this.guid.Length; i++)
            {
                this.guid.Set(i, guid[i]);
                if (i == guid.Length-1) break;
            }
        }
        
        public static implicit operator ContentGUID(NetworkedContentGUID ncguid) =>
            new ContentGUID(ncguid.guid.ToArray());
    }
}