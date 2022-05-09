using System;

namespace rwby
{
    [AddressablesContentParser("GameMode")]
    public class AddressablesGameModeParser : AddressablesContentParser<IGameModeDefinition>
    {
        public override int parserType
        {
            get { return (int)ContentType.Gamemode; }
        }
    }
}