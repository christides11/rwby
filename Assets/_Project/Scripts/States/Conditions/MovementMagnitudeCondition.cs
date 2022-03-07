using HnSF;
using HnSF.Fighters;

namespace rwby.state.conditions
{
    public class MovementMagnitudeCondition : StateConditionBase
    {
        public float minValidMagnitude;
        public float maxValidMagnitude;
        
        public override bool IsTrue(IFighterBase fm)
        {
            FighterManager manager = (FighterManager)fm;
            float movementMagnitude = manager.GetMovementVector().magnitude;
            bool result = (movementMagnitude < minValidMagnitude || movementMagnitude > maxValidMagnitude);
            return inverse ? result : !result;
        }
    }
}