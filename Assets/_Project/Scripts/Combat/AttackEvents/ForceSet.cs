using HnSF.Combat;
using HnSF.Fighters;
using rwby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.Combat.AttackEvents
{
    public class ForceSet : HnSF.Combat.AttackEvent
    {
        public bool applyXZForce;
        public bool applyYForce;

        public Vector2 xzForce;
        public float yForce;

        public override string GetName()
        {
            return "Set Forces";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame, IFighterBase controller)
        {
            FighterManager e = (FighterManager)controller;

            Vector3 f = Vector3.zero;
            if (applyXZForce)
            {
                f.x = xzForce.x;
                f.z = xzForce.y;
            }
            if (applyYForce)
            {
                f.y = yForce;
            }

            if (applyYForce)
            {
                e.PhysicsManager.forceGravity = f.y;
            }
            if (applyXZForce)
            {
                e.PhysicsManager.forceMovement = (f.x * e.transform.right) + (f.z * e.transform.forward);
            }
            return AttackEventReturnType.NONE;
        }
    }
}