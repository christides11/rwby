using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.core.content
{
    /*
    public class RRGunDashGround : FighterState
    {

        public override void Initialize()
        {
            (manager as RubyRoseManager).GundashCount = 0;
        }

        public override void OnUpdate()
        {

        }

        public override void OnLateUpdate()
        {
            MovesetAttackNode currentAttack = (MovesetAttackNode)manager.CombatManager.CurrentAttackNode;

            manager.StateManager.IncrementFrame();

            if (manager.StateManager.CurrentStateFrame >= 10 && (manager as RubyRoseManager).GundashCount < 2)
            {
                if (manager.InputManager.GetButton((PlayerInputType)currentAttack.inputSequence.executeInputs[0].buttonID, out int bOff).firstPress)
                {
                    manager.StateManager.CurrentStateFrame = 1;
                    (manager as RubyRoseManager).GundashCount++;
                }
            }
        }

        public override void OnInterrupted()
        {
            base.OnInterrupted();
        }

        public override bool CheckInterrupt()
        {
            return base.CheckInterrupt();
        }
    }*/
}