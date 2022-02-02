using HnSF.Combat;
using rwby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class SFlinchGround : FighterState
    {
        public override string GetName()
        {
            return $"Flinch (Ground)";
        }

        public override void Initialize()
        {
            base.Initialize();
            manager.fighterAnimator.Play("rr", "flinch.high");
        }

        public override void OnUpdate()
        {
            manager.BoxManager.UpdateBoxes((int)manager.StateManager.CurrentStateFrame, 0);

            if(manager.StateManager.CurrentStateFrame > manager.CombatManager.hitstunHoldVelocityTime)
            {
                manager.PhysicsManager.ApplyMovementFriction(manager.CombatManager.hitstunFriction);
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
            else if (manager.PhysicsManager.IsGrounded == false)
            {
                manager.StateManager.ChangeState((int)FighterCmnStates.FLINCH_AIR, manager.StateManager.CurrentStateFrame, false);
                return true;
            }
            return false;
        }
    }
}