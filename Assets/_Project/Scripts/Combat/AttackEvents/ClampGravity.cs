using HnSF.Combat;
using HnSF.Fighters;
using UnityEngine;

namespace rwby.Combat.AttackEvents
{
    public class ClampGravity : AttackEvent
    {
        public float maxLength;

        public override string GetName()
        {
            return "Clamp Gravity";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame, IFighterBase controller, AttackEventVariables variables)
        {
            FighterManager manager = controller as FighterManager;
            if (maxLength == 0)
            {
                manager.PhysicsManager.forceGravity = 0;
            }
            else
            {
                manager.PhysicsManager.forceGravity = Mathf.Clamp(manager.PhysicsManager.forceGravity, -maxLength, maxLength);
            }
            return AttackEventReturnType.NONE;
        }
    }
}