using HnSF;
using HnSF.Fighters;
using UnityEngine;

namespace rwby.state.conditions
{
    public class CompareFacingDirectionCondition : StateConditionBase
    {
        public float validMin;
        public float validMax;
        public bool validIfZero;
        
        public override bool IsTrue(IFighterBase fm)
        {
            FighterManager manager = fm as FighterManager;
            
            Vector3 movementVector = manager.GetMovementVector();
            if (movementVector.sqrMagnitude == 0) return validIfZero;
            
            float d = Vector3.Dot(manager.transform.forward.normalized, movementVector.normalized);
            
            bool result = d < validMin || d > validMax;
            return inverse ? result : !result;
        }
    }
}