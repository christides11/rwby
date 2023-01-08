namespace rwby
{
    [System.Serializable]
    public class AddressablesInfoFile
    {
        public string modName;
        public string modIdentifier;
        public ModBackingType backingType;
        public bool disableRequiresRestart;
        public bool enableRequiresRestart;
    }
}