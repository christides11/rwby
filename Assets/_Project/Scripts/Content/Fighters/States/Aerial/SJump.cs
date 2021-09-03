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
            manager.PhysicsManager.forceGravity = (2 * manager.StatManager.MaxJumpHeight) / manager.apexTime;
        }

        public override void OnUpdate()
        {
            manager.PhysicsManager.forceGravity += manager.gravity * manager.Runner.DeltaTime;
            manager.PhysicsManager.forceGravity = Mathf.Clamp(manager.PhysicsManager.forceGravity, -manager.StatManager.MaxFallSpeed, float.MaxValue);

            manager.PhysicsManager.HandleMovement(manager.StatManager.AerialBaseAcceleration, manager.StatManager.AerialAcceleration, manager.StatManager.AerialDeceleration,
                manager.StatManager.AerialMaxSpeed, manager.StatManager.AerialAccelFromDot);

            if (CheckInterrupt()) return;
            manager.StateManager.IncrementFrame();
        }

        public override bool CheckInterrupt()  
        {
            int bOff = 0;
            if(manager.TryAttack() || manager.TryAirDash() || manager.TryAirJump())
            {
                return true;
            }
            if (manager.StateManager.CurrentStateFrame > (int)(manager.apexTime * 60) 
                || manager.StateManager.CurrentStateFrame > manager.StatManager.MinJumpFrames 
                && manager.InputManager.GetJump(out bOff).isDown == false)
            {
                manager.StateManager.ChangeState((ushort)FighterCmnStates.FALL);
                return true;
            }
            return false;
        }
    }
}