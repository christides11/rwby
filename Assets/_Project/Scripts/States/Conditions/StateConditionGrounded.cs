using HnSF;
using HnSF.Fighters;

namespace rwby
{
    public class StateConditionGrounded : StateConditionBase
    {
        public override bool IsTrue(IFighterBase fm)
        {
            bool grounded = fm.PhysicsManager.IsGrounded;
            return inverse ? !grounded : grounded;
        }
    }
}