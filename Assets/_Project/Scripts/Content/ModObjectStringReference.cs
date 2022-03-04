namespace rwby
{
    [System.Serializable]
    public struct ModObjectStringReference
    {
        public ModIdentifierTuple modIdentifier;
        public string objectIdentifier;
        
        public ModObjectStringReference((byte, uint) modIdentifier, string objectIdentifier)
        {
            this.modIdentifier = new ModIdentifierTuple(){ Item1 = modIdentifier.Item1, Item2 = modIdentifier.Item2};
            this.objectIdentifier = objectIdentifier;
        }

        public ModObjectStringReference(ModIdentifierTuple modIdentifier, string objectIdentifier)
        {
            this.modIdentifier = modIdentifier;
            this.objectIdentifier = objectIdentifier;
        }

        public bool IsValid()
        {
            if (modIdentifier.Item2 == 0 || string.IsNullOrEmpty(objectIdentifier)) return false;
            return true;
        }

        public override string ToString()
        {
            return $"{modIdentifier.Item1}:{modIdentifier.Item2}/{objectIdentifier}";
        }
    }
}