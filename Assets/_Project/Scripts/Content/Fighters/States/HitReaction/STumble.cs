using HnSF.Combat;
using rwby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class STumble : FighterState
    {
        public override string GetName()
        {
            return $"Tumble";
        }

        public override void Initialize()
        {
            base.Initialize();
            manager.apexTime = manager.StatManager.MaxJumpTime / 2.0f;
            manager.gravity = (-2.0f * manager.StatManager.MaxJumpHeight) / Mathf.Pow(manager.apexTime, 2.0f);
            manager.CombatManager.hitstunGravityHangTimer = 0;
        }

        public override void OnUpdate()
        {
            manager.BoxManager.UpdateBoxes((int)manager.StateManager.CurrentStateFrame, 0);

            if (manager.PhysicsManager.forceGravity == 0 && manager.CombatManager.hitstunGravityHangTimer < manager.CombatManager.hitstunGravityHangTime)
            {
                manager.CombatManager.hitstunGravityHangTimer++;
            }
            else
            {
                manager.PhysicsManager.forceGravity += manager.gravity * manager.Runner.DeltaTime;
                manager.PhysicsManager.forceGravity = Mathf.Clamp(manager.PhysicsManager.forceGravity, -manager.StatManager.MaxFallSpeed, float.MaxValue);
                /*
                manager.PhysicsManager.HandleGravity(
                    manager.StatManager.hitstunGravity,
                    (manager.CombatManager as FighterCombatManager).hitstunGravity,
                    1);*/

                if (manager.CombatManager.hitstunGravityHangTimer < manager.CombatManager.hitstunGravityHangTime && Mathf.Abs(manager.PhysicsManager.forceGravity) <= 0.5f)
                {
                    manager.PhysicsManager.forceGravity = 0;
                }
            }

            if (CheckInterrupt() == false)
            {
                manager.StateManager.IncrementFrame();
            }
        }

        public override void OnInterrupted()
        {
            base.OnInterrupted();
            manager.CombatManager.hitstunGravityHangTimer = 0;
        }

        public override bool CheckInterrupt()
        {
            manager.PhysicsManager.CheckIfGrounded();            
            if (manager.PhysicsManager.forceGravity < 0 && manager.PhysicsManager.IsGrounded == true)
            {
                manager.StateManager.ChangeState((int)FighterCmnStates.GROUND_LAY);
                return true;
            }

            if (manager.StateManager.CurrentStateFrame > manager.CombatManager.HitStun)
            {

            }
            return false;
        }
    }
}