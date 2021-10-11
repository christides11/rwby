using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.fighters.states
{
    public class SGroundLay : FighterState
    {
        public override string GetName()
        {
            return "Ground Lay";
        }

        public override void Initialize()
        {
            base.Initialize();
            manager.ResetVariablesOnGround();
            manager.PhysicsManager.SetGrounded(true);
        }

        public override void OnUpdate()
        {
            //manager.HurtboxManager.CreateHurtboxes(0, 0);
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
            if (manager.StateManager.CurrentStateFrame >= 50)
            {
                manager.StateManager.ChangeState((ushort)FighterCmnStates.GROUND_GETUP);
                return true;
            }

            if (manager.CombatManager.HardKnockdown == true) return false;

            if (manager.StateManager.CurrentStateFrame <= 5)
            {
                if (manager.InputManager.GetLightAttack(out int bOff).isDown)
                {
                    manager.StateManager.ChangeState((ushort)FighterCmnStates.TECH_GROUND);
                    return true;
                }
            }
            return false;
        }
    }
}