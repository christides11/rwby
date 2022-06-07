using System;

namespace rwby
{
    [System.Serializable]
    public struct ModObjectSetContentReference : IEquatable<ModObjectSetContentReference>
    {
        public ContentGUID modGUID;
        public ContentGUID contentGUID;

        public ModObjectSetContentReference(ContentGUID modGUID, ContentGUID contentGUID)
        {
            this.modGUID = modGUID;
            this.contentGUID = contentGUID;
        }

        public ModObjectSetContentReference(byte[] modGUID, byte[] contentGUID)
        {
            this.modGUID = new ContentGUID(modGUID);
            this.contentGUID = new ContentGUID(contentGUID);
        }

        public override string ToString()
        {
            return $"{modGUID.ToString()}:?:{contentGUID}";
        }

        public bool Equals(ModObjectSetContentReference other)
        {
            return modGUID.Equals(other.modGUID) && contentGUID.Equals(other.contentGUID);
        }

        public override bool Equals(object obj)
        {
            return obj is ModObjectSetContentReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(modGUID, contentGUID);
        }
        
        public static bool operator ==(ModObjectSetContentReference x, ModObjectSetContentReference y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(ModObjectSetContentReference x, ModObjectSetContentReference y)
        {
            return !(x == y);
        }
        
        //public static implicit operator NetworkModObjectSetContentReference(ModObjectGUIDReference nmo) =>
        //    new NetworkModObjectGUIDReference(nmo.modGUID, nmo.contentType, nmo.contentGUID);
    }
}