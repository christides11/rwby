using System;

namespace rwby
{
    [AddressablesContentParser("Animationbank")]
    public class AddressablesAnimationbankParser : AddressablesContentParser<IAnimationbankDefinition>
    {
        public override int parserType
        {
            get { return (int)ContentType.Animationbank; }
        }
    }
}