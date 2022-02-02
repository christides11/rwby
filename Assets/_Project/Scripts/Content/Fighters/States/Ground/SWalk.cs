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
            manager.fighterAnimator.Play("rr", "walk.f");
        }

        public override void OnUpdate()
        {
            manager.BoxManager.UpdateBoxes(0, 0);
            Vector3 movement = manager.GetMovementVector();
            movement.y = 0;
            if (manager.HardTargeting)
            {
                Vector3 faceDir = (manager.CurrentTarget.transform.position - manager.transform.position).normalized;
                faceDir.y = 0;
                manager.RotateVisual(faceDir, manager.StatManager.walkRotationSpeed);
            }
            else
            {
                manager.RotateVisual(movement.normalized, manager.StatManager.walkRotationSpeed);
            }

            manager.PhysicsManager.HandleMovement(manager.StatManager.WalkBaseAccel, manager.StatManager.WalkAcceleration,
                manager.StatManager.GroundFriction, manager.StatManager.WalkMinSpeed, manager.StatManager.WalkMaxSpeed, manager.StatManager.WalkAccelFromDot);
        }

        public override void OnLateUpdate()
        {
            if (CheckInterrupt() == false)
            {
                manager.StateManager.IncrementFrame();
                manager.fighterAnimator.currentFrame++;
                if (manager.fighterAnimator.currentFrame >= 90)
                {
                    manager.fighterAnimator.currentFrame = 10;
                }
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