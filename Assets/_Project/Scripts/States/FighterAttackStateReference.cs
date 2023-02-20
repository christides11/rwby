namespace rwby
{
    public class FighterAttackStateReference : HnSF.FighterStateReferenceBase
    {
        public FighterAttackStates state = FighterAttackStates.GRD_5L;

        public override int GetState()
        {
            return (int)state;
        }
    }
}