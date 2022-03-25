using HnSF;
using HnSF.Fighters;

namespace rwby.state.conditions
{
    public class HitCountCondition : StateConditionBase
    {
        public int definitionIndex;
        public int minHits = 1;
        public int maxHits = 100;
        
        public override bool IsTrue(IFighterBase fm)
        {
            FighterManager manager = fm as FighterManager;
            int hitCount = manager.FCombatManager.HitboxManager.hitboxGroupHitCounts.Get(definitionIndex);
            bool result = hitCount >= minHits && hitCount <= maxHits;
            return inverse ? !result : result;
        }
    }
}