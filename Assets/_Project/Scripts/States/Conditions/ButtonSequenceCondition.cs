using HnSF;
using HnSF.Fighters;
using HnSF.Input;

namespace rwby.state.conditions
{
    public class ButtonSequenceCondition : StateConditionBase
    {
        public InputSequence sequence;
        public int offset = 0;
        public bool processSequenceButtons;
        public bool holdInput;
        
        public override bool IsTrue(IFighterBase fm)
        {
            FighterManager manager = fm as FighterManager;
            bool result = manager.CombatManager.CheckForInputSequence(sequence, (uint)offset, processSequenceButtons, holdInput);

            return inverse ? !result : result;
        }
    }
}