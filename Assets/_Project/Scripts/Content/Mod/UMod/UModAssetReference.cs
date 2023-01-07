namespace rwby
{
    [System.Serializable]
    public class UModAssetReference
    {
        public UModAssetLookupTable lookupTable;
        public int tableID;
        
        public string Reference => lookupTable[tableID];
    }
}