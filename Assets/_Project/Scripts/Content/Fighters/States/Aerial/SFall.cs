using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.fighters.states
{
    public class SFall : FighterState
    {
        public override string GetName()
        {
            return "Fall";
        }

        public override void Initialize()
        {
            base.Initialize();
            if (manager.apexTime == 0)
            {
                manager.apexTime = manager.StatManager.MaxJumpTime / 2.0f;
                manager.gravity = (-2.0f * manager.StatManager.MaxJumpHeight) / Mathf.Pow(manager.apexTime, 2.0f);
            }
        }

        public override void OnUpdate()
        {
            manager.PhysicsManager.forceGravity += manager.gravity * manager.StatManager.fallGravityMultiplier * manager.Runner.DeltaTime;
            manager.PhysicsManager.forceGravity = Mathf.Clamp(manager.PhysicsManager.forceGravity, -manager.StatManager.MaxFallSpeed, float.MaxValue);

            manager.PhysicsManager.HandleMovement(manager.StatManager.AerialBaseAcceleration, manager.StatManager.AerialAcceleration, manager.StatManager.AerialDeceleration,
                manager.StatManager.AerialMaxSpeed, manager.StatManager.AerialAccelFromDot);

            if (CheckInterrupt() == false)
            {
                manager.StateManager.IncrementFrame();
            }
        }

        public override bool CheckInterrupt()
        {
            manager.PhysicsManager.CheckIfGrounded();
            if (manager.PhysicsManager.IsGroundedNetworked == true)
            {
                if (manager.InputManager.GetMovement(0).magnitude >= InputConstants.movementDeadzone)
                {
                    manager.StateManager.ChangeState(manager.StoredRun ? (ushort)FighterCmnStates.RUN : (ushort)FighterCmnStates.WALK);
                }
                else
                {
                    manager.StateManager.ChangeState((ushort)FighterCmnStates.IDLE);
                }
                return true;
            }
            if (manager.TryAttack() || manager.TryAirDash() || manager.TryAirJump())
            {
                return true;
            }
            return false;
        }
    }
}