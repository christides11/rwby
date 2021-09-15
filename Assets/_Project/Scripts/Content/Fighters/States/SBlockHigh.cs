using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.fighters.states
{
    public class SBlockHigh : FighterState
    {
        public override string GetName()
        {
            return "Block (High)";
        }

        public override void Initialize()
        {
            base.Initialize();
            manager.ResetVariablesOnGround();
            manager.CombatManager.BlockState = BlockStateType.HIGH;
        }

        public override void OnUpdate()
        {
            manager.HurtboxManager.CreateHurtboxes(0, 0);
            manager.PhysicsManager.ApplyMovementFriction();

            Vector3 movement = manager.GetMovementVector();
            movement.y = 0;
            if(movement.magnitude >= InputConstants.movementDeadzone)
            {
                manager.SetVisualRotation(movement.normalized);
            }

            if (CheckInterrupt() == false)
            {
                manager.StateManager.IncrementFrame();
            }
        }

        public override void OnInterrupted()
        {
            manager.HurtboxManager.ResetHurtboxes();
            manager.CombatManager.BlockState = BlockStateType.NONE;
        }

        public override bool CheckInterrupt()
        {
            manager.PhysicsManager.CheckIfGrounded();
            if (manager.PhysicsManager.IsGroundedNetworked == false)
            {
                if (manager.InputManager.GetBlock(out int boff).isDown || manager.CombatManager.BlockStun > 0)
                {
                    manager.StateManager.ChangeState((ushort)FighterCmnStates.BLOCK_AIR);
                }
                else
                {
                    manager.StateManager.ChangeState((ushort)FighterCmnStates.FALL);
                }
                return true;
            }

            if (manager.CombatManager.BlockStun == 0)
            {
                if (manager.TryJump())
                {
                    return true;
                }
                if(manager.InputManager.GetBlock(out int bOff).isDown == false)
                {
                    manager.StateManager.ChangeState((ushort)FighterCmnStates.IDLE);
                    return true;
                }
            }
            return false;
        }
    }
}