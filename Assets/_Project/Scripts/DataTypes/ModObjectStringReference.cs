namespace rwby
{
    [System.Serializable]
    public struct ModObjectStringReference
    {
        public ModIdentifierTuple modIdentifier;
        public string objectIdentifier;
        
        public ModObjectStringReference((byte, uint) modIdentifier, string objectIdentifier)
        {
            this.modIdentifier = new ModIdentifierTuple(){ source = modIdentifier.Item1, identifier = modIdentifier.Item2};
            this.objectIdentifier = objectIdentifier;
        }

        public ModObjectStringReference(ModIdentifierTuple modIdentifier, string objectIdentifier)
        {
            this.modIdentifier = modIdentifier;
            this.objectIdentifier = objectIdentifier;
        }

        public bool IsValid()
        {
            if (modIdentifier.identifier == 0 || string.IsNullOrEmpty(objectIdentifier)) return false;
            return true;
        }

        public override string ToString()
        {
            return $"{modIdentifier.source}:{modIdentifier.identifier}/{objectIdentifier}";
        }
    }
}