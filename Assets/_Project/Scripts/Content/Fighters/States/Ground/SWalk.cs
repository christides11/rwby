using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.fighters.states
{
    public class SWalk : FighterState
    {
        public override string GetName()
        {
            return "Walk";
        }

        public override void Initialize()
        {
            base.Initialize();
            manager.ResetVariablesOnGround();
        }

        public override void OnUpdate()
        {
            manager.HurtboxManager.CreateHurtboxes(0, 0);
            Vector3 movement = manager.GetMovementVector();
            movement.y = 0;
            manager.RotateVisual(movement.normalized, manager.StatManager.walkRotationSpeed);

            manager.PhysicsManager.HandleMovement(manager.StatManager.WalkBaseAccel, manager.StatManager.WalkAcceleration,
                manager.StatManager.GroundFriction, manager.StatManager.WalkMaxSpeed, manager.StatManager.WalkAccelFromDot);

            if (CheckInterrupt() == false)
            {
                manager.StateManager.IncrementFrame();
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
            if (manager.TryBlock())
            {
                return true;
            }
            if (manager.TryAttack())
            {
                return true;
            }
            if (manager.InputManager.GetMovement(0).magnitude < InputConstants.movementDeadzone)
            {
                manager.StateManager.ChangeState((ushort)FighterCmnStates.IDLE);
                return true;
            }
            if(manager.InputManager.GetDash(out int butoff).firstPress == true)
            {
                manager.StateManager.ChangeState((ushort)FighterCmnStates.RUN);
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