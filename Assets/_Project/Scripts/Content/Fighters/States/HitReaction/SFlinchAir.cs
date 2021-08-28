using HnSF.Combat;
using rwby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class SFlinchAir : FighterState
    {
        public override string GetName()
        {
            return $"Flinch (Air)";
        }

        public override void Initialize()
        {
            base.Initialize();
            manager.CombatManager.hitstunGravityHangTimer = 0;
        }

        public override void OnUpdate()
        {
            manager.HurtboxManager.CreateHurtboxes((int)manager.StateManager.CurrentStateFrame, 0);

            if (manager.PhysicsManager.forceGravity == 0 && manager.CombatManager.hitstunGravityHangTimer < manager.CombatManager.hitstunGravityHangTime)
            {
                manager.CombatManager.hitstunGravityHangTimer++;
            }
            else
            {
                
                manager.PhysicsManager.HandleGravity(
                    manager.StatManager.MaxFallSpeed,
                    (manager.CombatManager as FighterCombatManager).hitstunGravity,
                    1);

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
            if (manager.StateManager.CurrentStateFrame > manager.CombatManager.HitStun)
            {
                manager.CombatManager.SetHitStun(0);
                // Hitstun finished.
                if (manager.PhysicsManager.IsGrounded)
                {
                    manager.StateManager.ChangeState((int)FighterCmnStates.IDLE);
                }
                else
                {
                    manager.StateManager.ChangeState((int)FighterCmnStates.FALL);
                }
            }
            else if (manager.PhysicsManager.IsGrounded == true)
            {
                manager.StateManager.ChangeState((int)FighterCmnStates.FLINCH_GROUND, manager.StateManager.CurrentStateFrame, false);
                return true;
            }
            return false;
        }
    }
}