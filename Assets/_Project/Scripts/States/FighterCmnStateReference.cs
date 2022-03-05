namespace rwby
{
    public class FighterCmnStateReference : FighterStateReferenceBase
    {
        public FighterCmnStates state = FighterCmnStates.IDLE;
        
        public override int GetState()
        {
            return (int)state;
        }
    }
}