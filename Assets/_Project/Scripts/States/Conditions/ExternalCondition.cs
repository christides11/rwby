using HnSF;
using HnSF.Fighters;

namespace rwby.state.conditions
{
    public class ExternalCondition : StateConditionBase
    {
        public StateConditionSO stateCondition;
        
        public override bool IsTrue(IFighterBase fm)
        {
            bool value = stateCondition.conditon.IsTrue(fm);
            return inverse ? !value : value;
        }
    }
}