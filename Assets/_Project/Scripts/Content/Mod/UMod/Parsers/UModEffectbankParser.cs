using System;

namespace rwby
{
    [UModContentParser("Effectbank", "Effectbanks")]
    [System.Serializable]
    public class UModEffectbankParser: UModContentParser<IEffectbankDefinition>
    {
        public override int parserType
        {
            get { return (int)ContentType.Effectbank; }
        }
    }
}