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
        
        public static bool CanAirDash(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3,
            int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionCanAirDash vars = (ConditionCanAirDash)variables;

            return f.CurrentAirDash < vars.maxAirDashes.GetValue(f);
        }
        
        public static bool Moveset(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3,
            int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionMoveset vars = (ConditionMoveset)variables;

            return f.StateManager.CurrentStateMoveset == vars.moveset;
        }

        public static bool HitstunValue(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionHitstunValue vars = (ConditionHitstunValue)variables;

            return f.FCombatManager.HitStun >= vars.minValue && f.FCombatManager.HitStun <= vars.maxValue;
        }

        public static bool BlockstunValue(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionBlockstunValue vars = (ConditionBlockstunValue)variables;

            return f.FCombatManager.BlockStun >= vars.minValue && f.FCombatManager.BlockStun <= vars.maxValue;
        }
        
        public static bool LockedOn(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionLockedOn vars = (ConditionLockedOn)variables;

            return vars.inverse ? !f.HardTargeting : f.HardTargeting;
        }
        
        public static bool WallValid(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionWallValid vars = (ConditionWallValid)variables;

            bool result = f.cWallNormal == Vector3.zero ? false : true;
            return vars.inverse ? !result : result;
        }
        
        public static bool HoldingTowardsWall(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionHoldingTowardsWall vars = (ConditionHoldingTowardsWall)variables;

            bool result = false;
            for (int i = 0; i < vars.buffer; i++)
            {
                Vector3 temp = f.GetMovementVector(i);
                Vector3 tempNormal = f.cWallNormal;
                tempNormal.y = 0;
                tempNormal.Normalize();
                if (temp == Vector3.zero) continue;
                if (Vector3.Angle(temp, -tempNormal) < 80)
                {
                    result = true;
                    break;
                }
            }
            
            return vars.inverse ? !result : result;
        }
        
        public static bool HitboxHitCount(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionHitboxHitCount vars = (ConditionHitboxHitCount)variables;

            bool result = f.FCombatManager.HitboxManager.hitboxGroupHitCounts[vars.hitboxIndex] >= vars.hitCount;
            return vars.inverse ? !result : result;
        }
        
        public static bool HitCount(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionHitCount vars = (ConditionHitCount)variables;

            int hitCount = 0;
            for (int i = 0; i < f.FCombatManager.HitboxManager.hitboxGroupHitCounts.Length; i++)
            {
                hitCount += f.FCombatManager.HitboxManager.hitboxGroupHitCounts[i];
                if (hitCount >= vars.hitCount) return true;
            }
            return false;
        }
        
        public static bool FloorAngle(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionFloorAngle vars = (ConditionFloorAngle)variables;

            if (f.groundSlopeAngle >= vars.minAngle && f.groundSlopeAngle <= vars.maxAngle) return true;
            return false;
        }
        
        public static bool CompareSlopeDir(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionCompareSlopeDir vars = (ConditionCompareSlopeDir)variables;

            if (f.groundSlopeAngle == 0) return false;
            
            Vector3 dir = Vector3.zero;
            switch (vars.inputSource)
            {
                case VarInputSourceType.stick:
                    dir = f.GetMovementVector(0);
                    break;
                case VarInputSourceType.rotation:
                    dir = f.transform.forward;
                    break;
            }
            if (dir == Vector3.zero) dir = f.transform.forward;
            dir.y = 0;
            dir.Normalize();

            var floorVector = f.groundSlopeDir;
            floorVector.y = 0;
            floorVector.Normalize();

            if (Vector3.Angle(dir, floorVector) <= vars.maxAngle) return true;
            return false;
        }
        
        public static bool PoleValid(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionPoleValid vars = (ConditionPoleValid)variables;

            bool result = f.foundPole == null ? false : true;
            return vars.inverse ? !result : result;
        }
        
        public static bool HasThrowees(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;

            return f.throwees[0] != null ? true : false;
        }
        
        public static bool CompareInputDir(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionCompareInputDir vars = (ConditionCompareInputDir)variables;

            Vector3 angleA;
            Vector3 angleB;
            
            return true;
            //return f.throwees[0] != null ? true : false;
        }
    }
}