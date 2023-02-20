namespace rwby
{
    public class FighterCmnStateReference : HnSF.FighterStateReferenceBase
    {
        public FighterCmnStates state = FighterCmnStates.NULL;
        
        public override int GetState()
        {
            return (int)state;
        }
    }
}