using System.Collections;
using System.Collections.Generic;
using HnSF;
using HnSF.Fighters;
using UnityEngine;

namespace rwby
{
    public static class BaseStateConditionFunctions
    {
        public static bool NoCondition(IFighterBase fighter, IConditionVariables variables)
        {
            return true;
        }
        
        /*
        public static bool GroundedState(IFighterBase fighter, IConditionVariables variables)
        {
            ConditionGroundState vars = (ConditionGroundState)variables;

            bool r = fighter.PhysicsManager.IsGrounded;
            if (vars.inverse) r = !r;
            return r;
        }*/
    }
}