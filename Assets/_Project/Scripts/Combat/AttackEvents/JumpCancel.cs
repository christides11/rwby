using HnSF.Combat;
using HnSF.Fighters;
using UnityEngine;

namespace rwby.Combat.AttackEvents
{
    public class JumpCancel : AttackEvent
    {
        public override string GetName()
        {
            return "Jump Cancel";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame, IFighterBase controller)
        {
            if ((controller as FighterManager).TryJump())
            {
                return AttackEventReturnType.INTERRUPT;
            }
            return AttackEventReturnType.NONE;
        }
    }
}