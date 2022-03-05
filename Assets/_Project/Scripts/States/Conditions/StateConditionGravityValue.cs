using HnSF;
using HnSF.Fighters;

namespace rwby
{
    public class StateConditionGravityValue : StateConditionBase
    {
        public float minValue = 0;
        public float maxValue = 0;
        
        public override bool IsTrue(IFighterBase fm)
        {
            FighterManager manager = fm as FighterManager;
            bool value = (manager.FPhysicsManager.forceGravity >= minValue && manager.FPhysicsManager.forceGravity <= maxValue);
            return inverse ? !value : value;
        }
    }
}