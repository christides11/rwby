namespace rwby
{
    [System.Serializable]
    public class ModObjectReference
    {
        public string modIdentifier;
        public string objectIdentifier;

        public ModObjectReference()
        {

        }

        public ModObjectReference(string identifier)
        {
            string[] split = identifier.Split('/');
            if(split.Length >= 2)
            {
                modIdentifier = split[0];
                objectIdentifier = split[1];
            }
        }

        public ModObjectReference(string modIdentifier, string objectIdentifier)
        {
            this.modIdentifier = modIdentifier;
            this.objectIdentifier = objectIdentifier;
        }

        public override string ToString()
        {
            return $"{modIdentifier}/{objectIdentifier}";
        }
    }
}