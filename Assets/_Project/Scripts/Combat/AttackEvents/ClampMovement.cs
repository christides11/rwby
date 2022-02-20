using HnSF.Combat;
using HnSF.Fighters;
using UnityEngine;

namespace rwby.Combat.AttackEvents
{
    public class ClampMovement : HnSF.Combat.AttackEvent
    {
        public float maxLength;

        public override string GetName()
        {
            return "Clamp Movement";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame, IFighterBase controller)
        {
            FighterManager manager = controller as FighterManager;
            manager.FPhysicsManager.forceMovement = Vector3.ClampMagnitude(manager.FPhysicsManager.forceMovement, maxLength);
            return AttackEventReturnType.NONE;
        }
    }
}
