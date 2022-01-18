using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.fighters.states
{
    public class SRun : FighterState
    {
        public override string GetName()
        {
            return "Run";
        }

        public override void Initialize()
        {
            base.Initialize();
            manager.ResetVariablesOnGround();
            manager.StoredRun = true;
            manager.fighterAnimator.Play("rr", "run.f");
        }

        public override void OnUpdate()
        {
            Vector3 movement = manager.GetMovementVector();
            movement.y = 0;
            manager.RotateVisual(movement.normalized, manager.StatManager.runRotationSpeed);

            manager.PhysicsManager.HandleMovement(manager.StatManager.RunBaseAccel, manager.StatManager.RunAcceleration,
                manager.StatManager.GroundFriction, manager.StatManager.RunMinSpeed, manager.StatManager.RunMaxSpeed, manager.StatManager.RunAccelFromDot);

            if (CheckInterrupt() == false)
            {
                manager.StateManager.IncrementFrame();
                manager.fighterAnimator.currentFrame++;
            }
        }

        public override bool CheckInterrupt()
        {
            manager.PhysicsManager.CheckIfGrounded();
            if (manager.PhysicsManager.IsGroundedNetworked == false)
            {
                manager.StateManager.ChangeState((ushort)FighterCmnStates.FALL);
                return true;
            }
            if (manager.TryAttack())
            {
                return true;
            }
            if (manager.InputManager.GetMovement(0).magnitude < InputConstants.movementDeadzone)
            {
                manager.StoredRun = false;
                manager.StateManager.ChangeState((ushort)FighterCmnStates.RUN_BRAKE);
                return true;
            }
            if (manager.TryJump())
            {
                return true;
            }
            return false;
        }
    }
}