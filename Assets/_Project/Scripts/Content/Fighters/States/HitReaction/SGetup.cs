using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.fighters.states
{
    public class SGetup : FighterState
    {
        public override string GetName()
        {
            return "Getup";
        }

        public override void Initialize()
        {
            Debug.Log("Getup.");
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

        public override void OnInterrupted()
        {
            manager.HurtboxManager.ResetHurtboxes();
        }

        public override bool CheckInterrupt()
        {
            if (manager.StateManager.CurrentStateFrame >= 15)
            {
                manager.StateManager.ChangeState((ushort)FighterCmnStates.IDLE);
                return true;
            }
            return false;
        }
    }
}