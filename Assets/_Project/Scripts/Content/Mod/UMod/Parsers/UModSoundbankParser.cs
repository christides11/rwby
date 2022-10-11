using System;

namespace rwby
{
    [UModContentParser("Soundbank", "Soundbanks")]
    [System.Serializable]
    public class UModSoundbankParser : UModContentParser<ISoundbankDefinition>
    {
        public override int parserType
        {
            get { return (int)ContentType.Soundbank; }
        }
    }
}