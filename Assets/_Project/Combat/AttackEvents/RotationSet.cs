using HnSF.Combat;
using HnSF.Fighters;
using UnityEngine;

namespace rwby.Combat.AttackEvents
{
    public class RotationSet : HnSF.Combat.AttackEvent
    {
        public override string GetName()
        {
            return "Set Rotation";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame, IFighterBase controller)
        {
            FighterManager e = (FighterManager)controller;

            Vector3 moveDir = e.GetMovementVector();
            if(moveDir != Vector3.zero)
            {
                e.SetVisualRotation(moveDir);
            }

            return AttackEventReturnType.NONE;
        }
    }
}