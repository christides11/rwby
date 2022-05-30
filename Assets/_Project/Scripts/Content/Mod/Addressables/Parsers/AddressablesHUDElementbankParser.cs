using System;

namespace rwby
{
    [AddressablesContentParser("HUD/HUD Element Bank", "HUD Element Banks")]
    public class AddressablesHUDElementbankParser : AddressablesContentParser<IHUDElementbankDefinition>
    {
        public override int parserType
        {
            get { return (int)ContentType.HUDElementbank; }
        }
    }
}