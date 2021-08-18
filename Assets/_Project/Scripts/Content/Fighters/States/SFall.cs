using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.fighters.states
{
    public class SFall : FighterState
    {
        public override string GetName()
        {
            return "Fall";
        }

        public override void Initialize()
        {
            base.Initialize();
            manager.apexTime = manager.StatManager.MaxJumpTime / 2.0f;
            manager.gravity = (-2.0f * manager.StatManager.MaxJumpHeight) / Mathf.Pow(manager.apexTime, 2.0f);
        }

        public override void OnUpdate()
        {
            manager.PhysicsManager.forceGravity += manager.gravity * manager.fallMulti * manager.Runner.DeltaTime;
            manager.PhysicsManager.forceGravity = Mathf.Clamp(manager.PhysicsManager.forceGravity, -manager.StatManager.MaxFallSpeed, float.MaxValue);

            if (CheckInterrupt() == false)
            {
                manager.StateManager.IncrementFrame();
            }
        }

        public override bool CheckInterrupt()
        {
            manager.PhysicsManager.CheckIfGrounded();
            if (manager.PhysicsManager.IsGroundedNetworked == true)
            {
                manager.PhysicsManager.forceGravity = 0;
                manager.StateManager.ChangeState((ushort)FighterCmnStates.IDLE);
                return true;
            }
            return false;
        }
    }
}