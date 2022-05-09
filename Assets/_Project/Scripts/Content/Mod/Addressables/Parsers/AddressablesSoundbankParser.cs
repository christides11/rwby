using System;

namespace rwby
{
    [AddressablesContentParser("Soundbank")]
    public class AddressablesSoundbankParser : AddressablesContentParser<ISoundbankDefinition>
    {
        public override int parserType
        {
            get { return (int)ContentType.Soundbank; }
        }
    }
}