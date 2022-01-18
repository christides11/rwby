using System;

namespace rwby
{
    [AddressablesContentParser("Soundbank")]
    public class AddressablesSoundbankParser : AddressablesContentParser<ISoundbankDefinition>
    {
        public override Type parserType { get { return typeof(ISoundbankDefinition); } }
    }
}