using System;

namespace rwby
{
    [AddressablesContentParser("Soundbank", "Soundbanks")]
    public class AddressablesSoundbankParser : AddressablesContentParser<ISoundbankDefinition>
    {
        public override int parserType
        {
            get { return (int)ContentType.Soundbank; }
        }
    }
}