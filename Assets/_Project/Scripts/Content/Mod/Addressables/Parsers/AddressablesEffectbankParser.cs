using System;

namespace rwby
{
    [AddressablesContentParser("Effectbank")]
    public class AddressablesEffectbankParser : AddressablesContentParser<IEffectbankDefinition>
    {
        public override Type parserType { get { return typeof(IEffectbankDefinition); } }
    }
}