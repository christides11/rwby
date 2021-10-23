using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.fighters.states
{
    public class SGroundBounce : FighterState
    {
        public override string GetName()
        {
            return "Ground Bounce";
        }

        public override void Initialize()
        {
            base.Initialize();
            manager.ResetVariablesOnGround();
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
            manager.PhysicsManager.CheckIfGrounded();
            return false;
        }
    }
}