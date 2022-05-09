using System;

namespace rwby
{
    [AddressablesContentParser("Map")]
    public class AddressablesMapParser : AddressablesContentParser<IMapDefinition>
    {
        public override int parserType
        {
            get { return (int)ContentType.Map; }
        }
    }
}