using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.fighters.states
{
    public class STrip : FighterState
    {
        public override string GetName()
        {
            return "Trip";
        }

        public override void Initialize()
        {
            base.Initialize();
            manager.ResetVariablesOnGround();
            manager.PhysicsManager.IsGroundedNetworked = false;
        }

        public override void OnUpdate()
        {
            manager.BoxManager.UpdateBoxes(0, 0);
            manager.PhysicsManager.ApplyMovementFriction();

            if (CheckInterrupt() == false)
            {
                manager.StateManager.IncrementFrame();
            }
        }

        public override void OnInterrupted()
        {
            manager.BoxManager.ClearBoxes();
        }

        public override bool CheckInterrupt()
        {
            if (manager.StateManager.CurrentStateFrame > 15)
            {
                manager.PhysicsManager.CheckIfGrounded();
                if (manager.PhysicsManager.IsGrounded)
                {
                    manager.StateManager.ChangeState((ushort)FighterCmnStates.GROUND_LAY);
                    return true;
                }
            }
            return false;
        }
    }
}