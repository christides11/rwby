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
            manager.CombatManager.BlockState = BlockStateType.AIR;

            if (manager.apexTime == 0)
            {
                manager.apexTime = manager.StatManager.MaxJumpTime / 2.0f;
                manager.gravity = (-2.0f * manager.StatManager.MaxJumpHeight) / Mathf.Pow(manager.apexTime, 2.0f);
            }
            manager.guardEffect.PlayEffect(true, false);
        }

        public override void OnUpdate()
        {
            manager.BoxManager.UpdateBoxes(0, 0);
            manager.PhysicsManager.ApplyMovementFriction();

            manager.PhysicsManager.forceGravity += manager.gravity * manager.StatManager.fallGravityMultiplier * manager.Runner.DeltaTime;
            manager.PhysicsManager.forceGravity = Mathf.Clamp(manager.PhysicsManager.forceGravity, -manager.StatManager.MaxFallSpeed, float.MaxValue);

            if (CheckInterrupt() == false)
            {
                manager.StateManager.IncrementFrame();
            }
        }

        public override void OnInterrupted()
        {
            manager.BoxManager.ClearBoxes();
            manager.CombatManager.BlockState = BlockStateType.NONE;
            manager.guardEffect.StopEffect(ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public override bool CheckInterrupt()
        {
            if (manager.CombatManager.BlockStun == 0)
            {
                if (manager.InputManager.GetBlock(out int bOff).isDown == false)
                {
                    manager.StateManager.ChangeState((ushort)FighterCmnStates.FALL);
                    return true;
                }
            }

            manager.PhysicsManager.CheckIfGrounded();
            if (manager.PhysicsManager.IsGroundedNetworked == true)
            {
                manager.StateManager.ChangeState((ushort)FighterCmnStates.BLOCK_HIGH);
                return true;
            }
            return false;
        }
    }
}