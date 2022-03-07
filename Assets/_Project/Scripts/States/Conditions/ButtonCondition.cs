using System;
using HnSF;
using HnSF.Fighters;
using UnityEngine;

namespace rwby.state.conditions
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom("rwby")]
    public class ButtonCondition : StateConditionBase
    {
        public enum ButtonStateType
        {
            IsDown = 0,
            FirstPress = 1,
            Released = 2
        }
        public PlayerInputType button;
        public ButtonStateType buttonState;
        public int offset = 0;
        public int buffer = 0;
        
        public override bool IsTrue(IFighterBase fm)
        {
            FighterManager manager = fm as FighterManager;
            var result = manager.InputManager.GetButton(button, out int buttonOffset, offset, buffer);
            bool fResult = false;
            switch (buttonState)
            {
                case ButtonStateType.IsDown:
                    if (result.isDown) fResult = true;
                        break;
                case ButtonStateType.FirstPress:
                    if (result.firstPress) fResult = true;
                        break;
                case ButtonStateType.Released:
                    if (result.released) fResult = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return inverse ? !fResult : fResult;
        }
    }
}