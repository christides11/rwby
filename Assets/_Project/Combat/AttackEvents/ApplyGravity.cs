using HnSF.Combat;
using HnSF.Fighters;
using UnityEngine;

namespace rwby.Combat.AttackEvents
{
    public class ApplyGravity : HnSF.Combat.AttackEvent
    {
        public override string GetName()
        {
            return "Gravity";
        }

        public AnimationCurve gravityCurve;

        public override AttackEventReturnType Evaluate(int frame, int endFrame, IFighterBase controller)
        {
            FighterManager manager = controller as FighterManager;
            manager.apexTime = manager.StatManager.MaxAirJumpTime / 2.0f;
            manager.gravity = (-2.0f * manager.StatManager.MaxAirJumpHeight) / Mathf.Pow(manager.apexTime, 2.0f);

            manager.PhysicsManager.forceGravity += manager.gravity * gravityCurve.Evaluate((float)frame / (float)endFrame) * manager.Runner.DeltaTime;
            manager.PhysicsManager.forceGravity = Mathf.Clamp(manager.PhysicsManager.forceGravity, -manager.StatManager.MaxFallSpeed, float.MaxValue);
            return AttackEventReturnType.NONE;
        }
    }
}