using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.fighters.states
{
    public class SRunBrake : FighterState
    {
        public override string GetName()
        {
            return "Run Brake";
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void OnUpdate()
        {
            manager.PhysicsManager.ApplyMovementFriction();

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
            if (manager.StateManager.CurrentStateFrame > 4)
            {
                manager.StateManager.ChangeState((ushort)FighterCmnStates.WALK);
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