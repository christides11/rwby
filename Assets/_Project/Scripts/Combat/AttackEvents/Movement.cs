using HnSF.Combat;
using HnSF.Fighters;
using UnityEngine;

namespace rwby.Combat.AttackEvents
{
    public class Movement : HnSF.Combat.AttackEvent
    {
        public float baseAcceleration = 0;
        public float acceleration = 0;
        public float deceleration = 0;
        public float minSpeed = 0;
        public float maxSpeed = 0;
        public AnimationCurve accelFromDot;

        public override string GetName()
        {
            return "Movement";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame, IFighterBase controller)
        {
            FighterManager manager = (FighterManager)controller;
            //manager.FPhysicsManager.HandleMovement(baseAcceleration, acceleration, deceleration, minSpeed, maxSpeed, accelFromDot);

            return AttackEventReturnType.NONE;
        }
    }
}