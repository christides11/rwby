using System;

namespace rwby
{
    [AddressablesContentParser("GameMode")]
    public class AddressablesGameModeParser : AddressablesContentParser<IGameModeDefinition>
    {
        public override Type parserType { get { return typeof(IGameModeDefinition); } }
    }
}