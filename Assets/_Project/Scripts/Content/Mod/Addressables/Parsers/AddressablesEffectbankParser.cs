
using System;

namespace rwby
{
    [AddressablesContentParser("Effectbank")]
    public class AddressablesEffectbankParser : AddressablesContentParser<IEffectbankDefinition>
    {
        public override int parserType
        {
            get { return (int)ContentType.Effectbank; }
        }
    }
}