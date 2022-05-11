
using System;

namespace rwby
{
    [AddressablesContentParser("Effectbank", "Effectbanks")]
    public class AddressablesEffectbankParser : AddressablesContentParser<IEffectbankDefinition>
    {
        public override int parserType
        {
            get { return (int)ContentType.Effectbank; }
        }
    }
}