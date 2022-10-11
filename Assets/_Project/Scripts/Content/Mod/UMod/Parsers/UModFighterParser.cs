using System;

namespace rwby
{
    [UModContentParser("Fighter", "Fighters")]
    [System.Serializable]
    public class UModFighterParser : UModContentParser<IFighterDefinition>
    {
        public override int parserType
        {
            get { return (int)ContentType.Fighter; }
        }
    }
}