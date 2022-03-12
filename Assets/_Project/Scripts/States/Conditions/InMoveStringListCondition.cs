using HnSF;
using HnSF.Fighters;
using UnityEngine;

namespace rwby.state.conditions
{
    public class InMoveStringListCondition : StateConditionBase
    {
        public int moveset;

        [SelectImplementation(typeof(FighterStateReferenceBase))] [SerializeField, SerializeReference]
        public FighterStateReferenceBase state = new FighterCmnStateReference();

        public int maxUses = 1;
        
        public override bool IsTrue(IFighterBase fm)
        {
            FighterManager manager = fm as FighterManager;
            bool result = manager.FCombatManager.MovePossible(new MovesetStateIdentifier(){ movesetIdentifier = moveset, stateIdentifier = state.GetState() }, maxUses);
            return inverse ? !result : result;
        }
    }
}