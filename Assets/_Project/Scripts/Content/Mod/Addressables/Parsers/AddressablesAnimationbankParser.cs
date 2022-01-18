using System;

namespace rwby
{
    [AddressablesContentParser("Animationbank")]
    public class AddressablesAnimationbankParser : AddressablesContentParser<IAnimationbankDefinition>
    {
        public override Type parserType { get { return typeof(IAnimationbankDefinition); } }
    }
}