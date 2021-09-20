using HnSF.Combat;
using HnSF.Fighters;
using UnityEngine;

namespace rwby.Combat.AttackEvents
{
    public class SoftLockOn : AttackEvent
    {
        public override string GetName()
        {
            return "Soft Lock On";
        }

        public float rotSpeed = 10;

        public override AttackEventReturnType Evaluate(int frame, int endFrame, IFighterBase controller)
        {
            FighterManager manager = controller as FighterManager;
            Vector3 mDir = manager.GetMovementVector();

            // Pointing in direction using movement.
            if(mDir.sqrMagnitude > 0)
            {
                manager.RotateVisual(mDir, rotSpeed);
                return AttackEventReturnType.NONE;
            }

            if(manager.CurrentTarget != null)
            {
                mDir = manager.CurrentTarget.transform.position - manager.transform.position;
                mDir.y = 0;
                mDir.Normalize();
                manager.RotateVisual(mDir, rotSpeed);
            }

            return AttackEventReturnType.NONE;
        }
    }
}