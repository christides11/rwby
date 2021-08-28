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

        public override AttackEventReturnType Evaluate(int frame, int endFrame, IFighterBase controller, AttackEventVariables variables)
        {
            FighterManager manager = controller as FighterManager;
            /*
            (controller as FighterManager).PickSoftlockTarget();
            Vector2 move = controller.InputManager.GetAxis2D((int)PlayerInputType.MOVEMENT);
            Vector3 dir = Vector3.zero;

            if (move.magnitude >= InputConstants.movementThreshold)
            {
                dir = (controller as FighterManager).GetMovementVector(move.x, move.y);
                dir.Normalize();
                controller.RotateVisual(dir, rotSpeed);
                return AttackEventReturnType.NONE;
            }

            if ((controller as FighterManager).LockonTarget != null)
            {
                dir = ((controller as FighterManager).LockonTarget.transform.position - controller.transform.position);
                dir.y = 0;
                dir.Normalize();
                controller.RotateVisual(dir, rotSpeed);
            }*/

            return AttackEventReturnType.NONE;
        }
    }
}