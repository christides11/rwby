using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.fighters.states
{
    public class SJumpAir : FighterState
    {
        public override string GetName()
        {
            return "Air Jump";
        }

        public override void Initialize()
        {
            Vector3 movement = manager.GetMovementVector();
            if(movement.magnitude < InputConstants.movementDeadzone)
            {
                movement = Vector3.zero;
            }

            manager.apexTime = manager.StatManager.MaxAirJumpTime / 2.0f;
            manager.gravity = (-2.0f * manager.StatManager.MaxAirJumpHeight) / Mathf.Pow(manager.apexTime, 2.0f);
            manager.PhysicsManager.forceGravity = (2.0f * manager.StatManager.MaxAirJumpHeight) / manager.apexTime;

            manager.PhysicsManager.forceMovement *= manager.StatManager.AirJumpConversedMomentum;
            manager.PhysicsManager.forceMovement += movement * manager.StatManager.AirJumpHorizontalMomentum;

            SoundbankContainer soundbankContainer = manager.SoundbankContainer;
            soundbankContainer.PlaySound("global", "air-jump", 0.5f);
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

            if (CheckInterrupt() == false)
            {
                manager.StateManager.IncrementFrame();
            }
        }

        public override bool CheckInterrupt()
        {
            if (manager.StateManager.CurrentStateFrame > manager.StatManager.minAirJumpTime && (manager.TryAttack() || manager.TryAirDash() || manager.TryAirJump()))
            {
                return true;
            }
            if (manager.StateManager.CurrentStateFrame > (int)(manager.apexTime * 60))
            {
                manager.StateManager.ChangeState((ushort)FighterCmnStates.FALL);
                return true;
            }
            return false;
        }
    }
}