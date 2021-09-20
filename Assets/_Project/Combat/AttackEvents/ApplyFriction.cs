using HnSF.Combat;
using HnSF.Fighters;
using UnityEngine;

namespace rwby.Combat.AttackEvents
{
    public class ApplyFriction : HnSF.Combat.AttackEvent
    {
        public bool useXZFriction;
        public bool useYFriction;

        public float xzFriction;
        public float yFriction;

        public override string GetName()
        {
            return "Friction";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame, IFighterBase controller)
        {
            FighterManager manager = controller as FighterManager;
            if (useXZFriction)
            {
                manager.PhysicsManager.ApplyMovementFriction(xzFriction);
            }
            if (useYFriction)
            {
                manager.PhysicsManager.ApplyGravityFriction(yFriction);
            }
            return AttackEventReturnType.NONE;
        }
    }
}