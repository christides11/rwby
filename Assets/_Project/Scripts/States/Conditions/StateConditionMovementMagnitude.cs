using HnSF;
using HnSF.Fighters;

namespace rwby
{
    public class StateConditionMovementMagnitude : StateConditionBase
    {
        public float minMagnitude;
        public float maxMagnitude;
        
        public override bool IsTrue(IFighterBase fm)
        {
            FighterManager manager = (FighterManager)fm;
            float movementMagnitude = manager.GetMovementVector().magnitude;
            return movementMagnitude >= minMagnitude && movementMagnitude <= maxMagnitude;
        }
    }
}