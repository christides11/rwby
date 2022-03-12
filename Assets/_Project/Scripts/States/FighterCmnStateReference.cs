namespace rwby
{
    public class FighterCmnStateReference : HnSF.FighterStateReferenceBase
    {
        public FighterCmnStates state = FighterCmnStates.IDLE;
        
        public override int GetState()
        {
            return (int)state;
        }
    }
}