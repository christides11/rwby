using System;

namespace rwby
{
    [AddressablesContentParser("Fighter")]
    public class AddressablesFighterParser : AddressablesContentParser<IFighterDefinition>
    {
        public override Type parserType { get { return typeof(IFighterDefinition); } }
    }
}