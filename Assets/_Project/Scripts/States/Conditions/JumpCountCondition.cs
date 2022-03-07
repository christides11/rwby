using HnSF;
using HnSF.Fighters;

namespace rwby.state.conditions
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom("rwby")]
    public class JumpCountCondition : StateConditionBase
    {
        public int minExpectedValue;
        public int maxExpectedValue;
        
        public override bool IsTrue(IFighterBase fm)
        {
            FighterManager manager = fm as FighterManager;
            bool result = !(manager.CurrentJump < minExpectedValue || manager.CurrentJump > maxExpectedValue);
            return inverse ? !result : result;
        }
    }
}