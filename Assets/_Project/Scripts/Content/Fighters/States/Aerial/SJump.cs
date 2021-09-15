using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.fighters.states
{
    public class SJump : FighterState
    {
        public override string GetName()
        {
            return "Jump";
        }

        public override void Initialize()
        {
            base.Initialize();
            manager.PhysicsManager.forceGravity = (2 * manager.StatManager.MaxJumpHeight) / manager.apexTime;

            manager.PhysicsManager.forceMovement *= manager.StatManager.JumpConversedMomentum;
            manager.PhysicsManager.forceMovement += manager.GetMovementVector() * manager.StatManager.JumpHorizontalVelocity;
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
            manager.PhysicsManager.CheckIfGrounded();
            int bOff = 0;
            if(manager.TryAttack() || manager.TryAirDash() || manager.TryAirJump() || manager.TryBlock())
            {
                return true;
            }
            if (manager.StateManager.CurrentStateFrame > (int)(manager.apexTime * 60) 
                || manager.StateManager.CurrentStateFrame > manager.StatManager.MinJumpFrames 
                && manager.InputManager.GetJump(out bOff).isDown == false)
            {
                manager.StateManager.ChangeState((ushort)FighterCmnStates.FALL);
                return true;
            }
            return false;
        }
    }
}