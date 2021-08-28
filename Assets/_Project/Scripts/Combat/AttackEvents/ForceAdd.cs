using HnSF.Combat;
using HnSF.Fighters;
using UnityEngine;

namespace rwby.Combat.AttackEvents
{
    public class ForceAdd : HnSF.Combat.AttackEvent
    {
        public bool addXZForce;
        public bool addYForce;

        public Vector2 xzForce;
        public float yForce;

        public override string GetName()
        {
            return "Add Forces";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame, IFighterBase controller, AttackEventVariables variables)
        {
            FighterManager e = (FighterManager)controller;

            Vector3 f = Vector3.zero;
            if (addXZForce)
            {
                f.x = xzForce.x;
                f.z = xzForce.y;
            }
            if (addYForce)
            {
                f.y = yForce;
            }

            if (addYForce)
            {
                e.PhysicsManager.forceGravity += f.y;
            }
            if (addXZForce)
            {
                e.PhysicsManager.forceMovement += (f.x * e.transform.right) + (f.z * e.transform.forward);
            }
            return AttackEventReturnType.NONE;
        }
    }
}