using System;

namespace rwby
{
    [AddressablesContentParser("Map", "Maps")]
    public class AddressablesMapParser : AddressablesContentParser<IMapDefinition>
    {
        public override int parserType
        {
            get { return (int)ContentType.Map; }
        }
    }
}