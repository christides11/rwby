using HnSF;
using HnSF.Fighters;

namespace rwby.state.conditions
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom("rwby")]
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