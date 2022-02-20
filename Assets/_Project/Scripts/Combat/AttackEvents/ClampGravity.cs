using HnSF.Combat;
using HnSF.Fighters;
using UnityEngine;

namespace rwby.Combat.AttackEvents
{
    public class ClampGravity : AttackEvent
    {
        public float max;
        public float min;

        public override string GetName()
        {
            return "Clamp Gravity";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame, IFighterBase controller)
        {
            FighterManager manager = controller as FighterManager;
            manager.FPhysicsManager.forceGravity = Mathf.Clamp(manager.FPhysicsManager.forceGravity, min, max);
            return AttackEventReturnType.NONE;
        }
    }
}