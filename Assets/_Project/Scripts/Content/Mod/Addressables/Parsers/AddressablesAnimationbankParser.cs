using System;

namespace rwby
{
    [AddressablesContentParser("Animationbank", "Animationbanks")]
    public class AddressablesAnimationbankParser : AddressablesContentParser<IAnimationbankDefinition>
    {
    
    
        public override int parserType
        {
            get { return (int)ContentType.Animationbank; }
        }
        
    }
}