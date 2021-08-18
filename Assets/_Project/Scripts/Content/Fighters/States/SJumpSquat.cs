using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.fighters.states
{
    public class SJumpSquat : FighterState
    {

        public override void Initialize()
        {
            base.Initialize();
            manager.apexTime = manager.StatManager.MaxJumpTime / 2.0f;
            manager.gravity = (-2.0f * manager.StatManager.MaxJumpHeight) / Mathf.Pow(manager.apexTime, 2.0f);
            manager.initialJumpVelocity = (2 * manager.StatManager.MaxJumpHeight) / manager.apexTime;
        }

        public override void OnUpdate()
        {
            if (CheckInterrupt() == false)
            {
                manager.StateManager.IncrementFrame();
            }
        }

        public override bool CheckInterrupt()
        {
            if (manager.StateManager.CurrentStateFrame > manager.StatManager.JumpSquatFrames)
            {
                manager.StateManager.ChangeState((ushort)FighterCmnStates.JUMP);
                return true;
            }
            return false;
        }
    }
}