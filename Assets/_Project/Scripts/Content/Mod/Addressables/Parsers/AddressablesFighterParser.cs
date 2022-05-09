using System;

namespace rwby
{
    [AddressablesContentParser("Fighter")]
    public class AddressablesFighterParser : AddressablesContentParser<IFighterDefinition>
    {
        public override int parserType
        {
            get { return (int)ContentType.Fighter; }
        }
    }
}