using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.fighters.states
{
    public class SBlockAir : FighterState
    {
        public override string GetName()
        {
            return "Block (Air)";
        }

        public override void Initialize()
        {
            base.Initialize();
            manager.ResetVariablesOnGround();
            manager.CombatManager.BlockState = BlockStateType.AIR;
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
            manager.CombatManager.BlockState = BlockStateType.NONE;
        }

        public override bool CheckInterrupt()
        {
            manager.PhysicsManager.CheckIfGrounded();
            if (manager.PhysicsManager.IsGroundedNetworked == false)
            {
                manager.StateManager.ChangeState((ushort)FighterCmnStates.FALL);
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