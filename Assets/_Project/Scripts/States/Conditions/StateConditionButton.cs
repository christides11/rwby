using System;
using HnSF;
using HnSF.Fighters;

namespace rwby
{
    public class StateConditionButton : StateConditionBase
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
            switch (buttonState)
            {
                case ButtonStateType.IsDown:
                    if (result.isDown) return true;
                    break;
                case ButtonStateType.FirstPress:
                    if (result.firstPress) return true;
                    break;
                case ButtonStateType.Released:
                    if (result.released) return true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return false;
        }
    }
}