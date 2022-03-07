using HnSF;
using HnSF.Fighters;

namespace rwby.state.conditions
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom("rwby")]
    public class GroundedCondition : StateConditionBase
    {
        public override bool IsTrue(IFighterBase fm)
        {
            bool grounded = fm.PhysicsManager.IsGrounded;
            return inverse ? !grounded : grounded;
        }
    }
}