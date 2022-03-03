using HnSF;
using HnSF.Fighters;

namespace rwby
{
    public class StateConditionExternal : StateConditionBase
    {
        public StateConditionSO stateCondition;
        
        public override bool IsTrue(IFighterBase fm)
        {
            return stateCondition.conditon.IsTrue(fm);
        }
    }
}