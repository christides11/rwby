using System;

namespace rwby
{
    [UModContentParser("HUDElementbank", "HUDElementbanks")]
    [System.Serializable]
    public class UModHUDElementbankParser : UModContentParser<IHUDElementbankDefinition>
    {
        public override int parserType
        {
            get { return (int)ContentType.HUDElementbank; }
        }
    }
}