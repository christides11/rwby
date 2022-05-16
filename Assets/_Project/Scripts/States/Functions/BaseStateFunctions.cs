using System.Collections;
using System.Collections.Generic;
using HnSF;
using HnSF.Fighters;
using UnityEngine;

namespace rwby
{
    public static class BaseStateFunctions
    {
        public static void Null(IFighterBase fighter, IStateVariables variables)
        {
            
        }
        
        public static void ChangeState(IFighterBase fighter, IStateVariables variables)
        {
            //State.ChangeState vars = (State.ChangeState)variables;
            //fighter.StateManager.MarkForStateChange(vars.stateID, vars.stateMovesetID);
        }
        
        public static void ApplyTraction(IFighterBase fighter, IStateVariables variables)
        {
            VarApplyTraction vars = (VarApplyTraction)variables;
            FighterManager fm = (FighterManager)fighter;
            if (vars.useTractionStat)
            {
                //fm.physicsManager.ApplyFriction(vars.aerialTraction ? fm.statManager.aerialTraction : fm.statManager.groundTraction);
            }
            else
            {
                //fm.physicsManager.ApplyFriction(vars.traction);
            }
        }
    }
}