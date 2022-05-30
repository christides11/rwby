using System;
using System.Collections;
using System.Collections.Generic;
using HnSF;
using HnSF.Fighters;
using UnityEngine;

namespace rwby
{
    public static class BaseStateConditionFunctions
    {
        public static bool NoCondition(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            return true;
        }

        public static bool MovementStickMagnitude(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionMoveStickMagnitude vars = (ConditionMoveStickMagnitude)variables;

            float movementSqrMagnitude = f.InputManager.GetMovement(0).sqrMagnitude;

            if (movementSqrMagnitude < (vars.minValue * vars.minValue) ||
                movementSqrMagnitude > (vars.maxValue * vars.maxValue)) return false;
            return true;
        }

        public static bool FallSpeed(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionFallSpeed vars = (ConditionFallSpeed)variables;

            float fallSpeed = f.FPhysicsManager.forceGravity;
            if (vars.absoluteValue) fallSpeed = Mathf.Abs(fallSpeed);

            if (fallSpeed < vars.minValue || fallSpeed > vars.maxValue) return false;
            return true;
        }
        
        public static bool IsGrounded(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionIsGrounded vars = (ConditionIsGrounded)variables;

            bool r = fighter.PhysicsManager.IsGrounded;
            if (vars.inverse) r = !r;
            return r;
        }

        public static bool Button(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionButton vars = (ConditionButton)variables;
            
            bool fResult = false;
            var b = f.InputManager.GetButton((int)vars.button, vars.offset, vars.buffer);
            switch (vars.buttonState)
            {
                case ConditionButton.ButtonStateType.IsDown:
                    if (b.isDown) fResult = true;
                    break;
                case ConditionButton.ButtonStateType.FirstPress:
                    if (b.firstPress) fResult = true;
                    break;
                case ConditionButton.ButtonStateType.Released:
                    if (b.released) fResult = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return vars.inverse ? !fResult : fResult;
        }

        public static bool ButtonSequence(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionButtonSequence vars = (ConditionButtonSequence)variables;
            
            return f.CombatManager.CheckForInputSequence(vars.sequence, (uint)vars.offset, vars.processSequenceButtons, vars.holdInput);
        }

        public static bool ANDCondition(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3,
            int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionAnd vars = (ConditionAnd)variables;

            for (int i = 0; i < vars.conditions.Length; i++)
            {
                if (!f.FStateManager.conditionMapper.TryCondition(vars.conditions[i].GetType(), f, vars.conditions[i],
                        arg3, arg4)) return false;
            }
            return true;
        }        
        
        public static bool ORCondition(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3,
            int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionOr vars = (ConditionOr)variables;

            for (int i = 0; i < vars.conditions.Length; i++)
            {
                if (f.FStateManager.conditionMapper.TryCondition(vars.conditions[i].GetType(), f, vars.conditions[i],
                        arg3, arg4)) return true;
            }
            return false;
        }
        
        public static bool CanAirJump(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3,
            int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionCanAirJump vars = (ConditionCanAirJump)variables;

            return f.CurrentJump < vars.maxAirJumps.GetValue(f);
        }
        
        public static bool Moveset(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3,
            int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionMoveset vars = (ConditionMoveset)variables;

            return f.StateManager.CurrentStateMoveset == vars.moveset;
        }
    }
}