using System;

namespace rwby
{
    [AddressablesContentParser("Fighter", "Fighters")]
    [System.Serializable]
    public class AddressablesFighterParser : AddressablesContentParser<IFighterDefinition>
    {
        public override int parserType
        {
            get { return (int)ContentType.Fighter; }
        }
    }
}