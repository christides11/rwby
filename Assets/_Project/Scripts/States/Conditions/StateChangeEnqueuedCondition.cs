using HnSF;
using HnSF.Fighters;

namespace rwby.state.conditions
{
    public class StateChangeEnqueuedCondition : StateConditionBase
    {
        public override bool IsTrue(IFighterBase fm)
        {
            FighterManager manager = (FighterManager)fm;
            bool result = manager.FStateManager.markedForStateChange;
            return inverse ? !result : result;
        }
    }
}