using HnSF;
using HnSF.Fighters;

namespace rwby.state.conditions
{
    public class GroundedCondition : StateConditionBase
    {
        public override bool IsTrue(IFighterBase fm)
        {
            bool grounded = fm.PhysicsManager.IsGrounded;
            return inverse ? !grounded : grounded;
        }
    }
}