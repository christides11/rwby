using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.core
{
    public class RRTeleport : FighterState
    {

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void OnUpdate()
        {
            if(manager.StateManager.CurrentStateFrame == 19)
            {
                bool buttonHeld = true;
                for(int i = 0; i < 19; i++)
                {
                    if(manager.InputManager.GetAbility3(out int buttonOff, i).isDown == false)
                    {
                        buttonHeld = false;
                        break;
                    }
                }

                if (manager.CurrentTarget)
                {
                    PickLockonPosition(buttonHeld);
                }
                else
                {

                }
            }

            if(CheckInterrupt() == false)
            {
                manager.StateManager.IncrementFrame();
            }
        }

        private void PickLockonPosition(bool buttonHeld)
        {

        }

        public override bool CheckInterrupt()
        {
            return base.CheckInterrupt();
        }
    }
}