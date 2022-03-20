using HnSF;
using HnSF.Fighters;

namespace rwby.state.conditions
{
    public class InHitstunCondition : StateConditionBase
    {
        public override bool IsTrue(IFighterBase fm)
        {
            FighterManager manager = fm as FighterManager;
            bool result = manager.CombatManager.HitStun > 0;
            return inverse ? !result : result;
        }
    }
}