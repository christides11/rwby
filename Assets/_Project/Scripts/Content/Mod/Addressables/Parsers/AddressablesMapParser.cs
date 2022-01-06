using System;

namespace rwby
{
    [AddressablesContentParser("Map")]
    public class AddressablesMapParser : AddressablesContentParser<IMapDefinition>
    {
        public override Type parserType { get { return typeof(IMapDefinition); } }
    }
}