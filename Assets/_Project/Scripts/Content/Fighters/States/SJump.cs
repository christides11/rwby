using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.fighters.states
{
    public class SJump : FighterState
    {

        public override void Initialize()
        {
            base.Initialize();
            manager.PhysicsManager.forceGravity = manager.initialJumpVelocity;
        }

        public override void OnUpdate()
        {
            manager.PhysicsManager.forceGravity += manager.gravity * manager.Runner.DeltaTime;
            manager.PhysicsManager.forceGravity = Mathf.Clamp(manager.PhysicsManager.forceGravity, -manager.StatManager.MaxFallSpeed, float.MaxValue);

            if (CheckInterrupt() == false)
            {
                manager.StateManager.IncrementFrame();
            }
        }

        public override bool CheckInterrupt()  
        {
            if (manager.StateManager.CurrentStateFrame > (int)(manager.apexTime * 60) 
                || manager.StateManager.CurrentStateFrame > manager.StatManager.MinJumpFrames 
                && manager.InputManager.GetJump(out int bOff).isDown == false)
            {
                manager.StateManager.ChangeState((ushort)FighterCmnStates.FALL);
                return true;
            }
            return false;
        }
    }
}