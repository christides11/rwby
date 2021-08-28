using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.fighters.states
{
    public class SIdle : FighterState
    {
        public override string GetName()
        {
            return "Idle";
        }

        public override void Initialize()
        {
            base.Initialize();
            manager.ResetVariablesOnGround();
        }

        public override void OnUpdate()
        {
            manager.HurtboxManager.CreateHurtboxes(0, 0);
            manager.PhysicsManager.ApplyMovementFriction();

            if (CheckInterrupt() == false)
            {
                manager.StateManager.IncrementFrame();
            }
        }

        public override void OnInterrupted()
        {
            manager.HurtboxManager.ResetHurtboxes();
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
            if(manager.InputManager.GetMovement(0).magnitude >= InputConstants.movementDeadzone)
            {
                if (manager.InputManager.GetDash(out int bOff).isDown)
                {
                    manager.StateManager.ChangeState((ushort)FighterCmnStates.RUN);
                }
                else
                {
                    manager.StateManager.ChangeState((ushort)FighterCmnStates.WALK);
                }
                return true;
            }
            if(manager.TryJump())
            {
                return true;
            }
            return false;
        }
    }
}