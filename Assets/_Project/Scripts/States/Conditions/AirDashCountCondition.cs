using HnSF;
using HnSF.Fighters;
using UnityEngine;

namespace rwby.state.conditions
{
    public class AirDashCountCondition : StateConditionBase
    {
        [SelectImplementation((typeof(FighterStatReferenceBase<int>)))] [SerializeReference]
        public FighterStatReferenceIntBase minExpectedValue = new FighterBaseStatReferenceInt();
        [SelectImplementation((typeof(FighterStatReferenceBase<int>)))] [SerializeReference]
        public FighterStatReferenceIntBase maxExpectedValue = new FighterBaseStatReferenceInt();
        
        public override bool IsTrue(IFighterBase fm)
        {
            FighterManager manager = fm as FighterManager;
            bool result = !(manager.CurrentAirDash < minExpectedValue.GetValue(manager) || manager.CurrentAirDash >= maxExpectedValue.GetValue(manager));
            return inverse ? !result : result;
        }
    }
}