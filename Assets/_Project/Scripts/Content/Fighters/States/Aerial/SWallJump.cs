using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.fighters.states
{
    public class SWallJump : FighterState
    {
        public override string GetName()
        {
            return "Wall Jump";
        }

        public override void Initialize()
        {
            base.Initialize();
            manager.apexTime = manager.StatManager.MaxWallJumpTime / 2.0f;
            manager.gravity = (-2.0f * manager.StatManager.MaxWallJumpHeight) / Mathf.Pow(manager.apexTime, 2.0f);
            manager.PhysicsManager.forceGravity = (2.0f * manager.StatManager.MaxWallJumpHeight) / manager.apexTime;

            manager.PhysicsManager.forceMovement *= manager.StatManager.WallJumpConversedMomentum;
            manager.PhysicsManager.forceMovement += manager.GetMovementVector() * manager.StatManager.WallJumpHorizontalMomentum;
        }

        public override void OnUpdate()
        {
            Vector3 movement = manager.GetMovementVector();
            movement.y = 0;
            manager.RotateVisual(movement.normalized, manager.StatManager.aerialRotationSpeed);

            manager.PhysicsManager.forceGravity += manager.gravity * manager.Runner.DeltaTime;
            manager.PhysicsManager.forceGravity = Mathf.Clamp(manager.PhysicsManager.forceGravity, -manager.StatManager.MaxFallSpeed, float.MaxValue);

            manager.PhysicsManager.HandleMovement(manager.StatManager.AerialBaseAcceleration, manager.StatManager.AerialAcceleration, manager.StatManager.AerialDeceleration,
                0, manager.StatManager.AerialMaxSpeed, manager.StatManager.AerialAccelFromDot);

            if (CheckInterrupt()) return;
            manager.StateManager.IncrementFrame();
        }

        public override bool CheckInterrupt()
        {
            if (manager.TryAttack() || manager.TryAirDash() || manager.TryAirJump())
            {
                return true;
            }
            if(manager.StateManager.CurrentStateFrame > 10 && manager.TryWallRun())
            {
                return true;
            }

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
            return false;
        }
    }
}