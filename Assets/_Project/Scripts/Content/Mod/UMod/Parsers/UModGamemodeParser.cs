using System;

namespace rwby
{
    [UModContentParser("Gamemode", "Gamemodes")]
    [System.Serializable]
    public class UModGamemodeParser : UModContentParser<IGameModeDefinition>
    {
        public override int parserType
        {
            get { return (int)ContentType.Gamemode; }
        }
    }
}