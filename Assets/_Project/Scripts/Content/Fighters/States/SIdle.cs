using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.fighters.states
{
    public class SIdle : FighterState
    {
        public override string GetName()
        {
            return "Idle";
        }

        public override void Initialize()
        {
            base.Initialize();
            manager.PhysicsManager.forceGravity = 0;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if(CheckInterrupt() == false)
            {
                manager.StateManager.IncrementFrame();
            }
        }

        public override bool CheckInterrupt()
        {
            if(manager.InputManager.GetJump(out int bOff).firstPress)
            {
                manager.StateManager.ChangeState((ushort)FighterCmnStates.JUMPSQUAT);
                return true;
            }
            return false;
        }
    }
}