using HnSF;

namespace rwby
{
    public class FighterCmnStateReference : HnSF.FighterStateReferenceBase
    {
        public FighterCmnStates state = FighterCmnStates.NULL;
        
        public override int GetState()
        {
            return (int)state;
        }

        public override FighterStateReferenceBase Copy()
        {
            return new FighterCmnStateReference()
            {
                state = state
            };
        }
    }
}