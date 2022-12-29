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
            var buttonInt = (int)vars.button;
            if (vars.checkAbilityButton)
            {
                switch (f.FCombatManager.lastUsedSpecial)
                {
                    case 0:
                        buttonInt = (int)PlayerInputType.ABILITY_1;
                        break;
                    case 1:
                        buttonInt = (int)PlayerInputType.ABILITY_2;
                        break;
                    case 2:
                        buttonInt = (int)PlayerInputType.ABILITY_3;
                        break;
                    case 3:
                        buttonInt = (int)PlayerInputType.ABILITY_4;
                        break;
                }
            }
            var b = f.InputManager.GetButton(buttonInt, vars.offset, vars.buffer);
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
            var vars = (ConditionBlockstunValue)variables;

            var result =(f.FCombatManager.BlockStun >= vars.minValue && f.FCombatManager.BlockStun <= vars.maxValue);
            return vars.inverse ? !result : result;
        }
        
        public static bool InBlockstun(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            var vars = (ConditionInBlockstun)variables;

            var result = f.FCombatManager.BlockStun > 0;
            return vars.inverse ? !result : result;
        }
        
        public static bool InHitstun(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            var vars = (ConditionInHitstun )variables;

            var result = f.FCombatManager.HitStun > 0;
            return vars.inverse ? !result : result;
        }
        
        public static bool LockedOn(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionLockedOn vars = (ConditionLockedOn)variables;

            if (vars.requireTarget && f.CurrentTarget == null) return vars.inverse ? true : false;
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
            ConditionHitOrBlockCount vars = (ConditionHitOrBlockCount)variables;

            int hitCount = 0;
            if (vars.hitOrBlock.HasFlag(ConditionHitOrBlockCount.HitOrBlockEnum.Hit))
            {
                for (int i = 0; i < f.FCombatManager.HitboxManager.hitboxGroupHitCounts.Length; i++)
                {
                    hitCount += f.FCombatManager.HitboxManager.hitboxGroupHitCounts[i];
                    if (hitCount >= vars.hitCount) return !vars.inverse;
                }
            }

            if (vars.hitOrBlock.HasFlag(ConditionHitOrBlockCount.HitOrBlockEnum.Block))
            {
                for (int i = 0; i < f.FCombatManager.HitboxManager.hitboxGroupBlockedCounts.Length; i++)
                {
                    hitCount += f.FCombatManager.HitboxManager.hitboxGroupBlockedCounts[i];
                    if (hitCount >= vars.hitCount) return !vars.inverse;
                }
            }

            return vars.inverse ? true : false;
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

            return f.FCombatManager.throwees[0] != null ? true : false;
        }
        
        public static bool CompareInputDir(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionCompareInputDir vars = (ConditionCompareInputDir)variables;

            Vector3 angleA = Vector3.forward;
            Vector3 angleB = Vector3.forward;

            switch (vars.inputSourceA)
            {
                case VarInputSourceType.stick:
                    angleA = f.GetMovementVector();
                    if (angleA == Vector3.zero) angleA = f.myTransform.forward;
                    break;
                case VarInputSourceType.slope:
                    angleA = f.groundSlopeDir;
                    angleA.y = 0;
                    angleA.Normalize();
                    break;
                case VarInputSourceType.rotation:
                    angleA = f.myTransform.forward;
                    break;
            }
            
            switch (vars.inputSourceB)
            {
                case VarInputSourceType.stick:
                    angleB = f.GetMovementVector();
                    if (angleA == Vector3.zero) angleA = f.myTransform.forward;
                    break;
                case VarInputSourceType.slope:
                    angleB = f.groundSlopeDir;
                    angleB.y = 0;
                    angleB.Normalize();
                    break;
                case VarInputSourceType.rotation:
                    angleB = f.myTransform.forward;
                    break;
            }

            var angle = vars.signedAngle ? Vector3.SignedAngle(angleA, angleB, Vector3.up) 
                : Vector3.Angle(angleA, angleB);
            if (angle >= vars.minAngle && angle <= vars.maxAngle) return !vars.inverse;
            return vars.inverse ? true : false;
        }
        
        public static bool WallAngle(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            if (f.cWallNormal == Vector3.zero) return false;
            ConditionWallAngle vars = (ConditionWallAngle)variables;

            Vector3 input = Vector3.zero;

            switch (vars.inputSource)
            {
                case VarInputSourceType.stick:
                    input = f.GetMovementVector();
                    break;
                case VarInputSourceType.slope:
                    input = f.groundSlopeDir;
                    input.y = 0;
                    input.Normalize();
                    break;
                case VarInputSourceType.rotation:
                    input = f.myTransform.forward;
                    break;
            }

            if (input == Vector3.zero) return false;
            
            var angle = Vector3.SignedAngle(input, -f.cWallNormal, Vector3.up);
            if (angle >= vars.minAngle && angle <= vars.maxAngle) return true;
            return false;
        }
        
        public static bool ChargeLevel(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionChargeLevel vars = (ConditionChargeLevel)variables;

            return f.FCombatManager.CurrentChargeLevel >= vars.minLevel &&
                   f.FCombatManager.CurrentChargeLevel <= vars.maxLevel;
        }
        
        public static bool CheckSuccessfulBlock(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionCheckSuccessfulBlock vars = (ConditionCheckSuccessfulBlock)variables;

            bool r =(f.Runner.Tick - f.FCombatManager.LastSuccessfulBlockTick) <= vars.checkDistance;
            return vars.inverse ? !r : r;
        }
        
        public static bool CanWallBounce(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3,
            int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionCanWallBounce vars = (ConditionCanWallBounce)variables;

            bool r = f.FCombatManager.WallBounce && f.FCombatManager.CurrentWallBounces < 2;
            return vars.inverse ? !r : r;
        }
        
        public static bool CanGroundBounce(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3,
            int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionCanGroundBounce vars = (ConditionCanGroundBounce)variables;

            bool r = f.FCombatManager.GroundBounce == true && f.FCombatManager.CurrentGroundBounces < 2;
            return vars.inverse ? !r : r;
        }
        
        public static bool WhiteboardIntIntComparison(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3,
            int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionWhiteboardIntIntComparison vars = (ConditionWhiteboardIntIntComparison)variables;

            int a = f.fighterWhiteboard.Ints[vars.valueAIndex];
            int b = f.fighterWhiteboard.Ints[vars.valueBIndex];

            switch (vars.comparison)
            {
                case ConditionWhiteboardIntIntComparison.ComparisonTypes.LESS_THAN:
                    if (a < b) return true;
                    break;
                case ConditionWhiteboardIntIntComparison.ComparisonTypes.LESS_THAN_OR_EQUAL:
                    if (a <= b) return true;
                    break;
                case ConditionWhiteboardIntIntComparison.ComparisonTypes.EQUAL:
                    if (a == b) return true;
                    break;
                case ConditionWhiteboardIntIntComparison.ComparisonTypes.GREATER_THAN:
                    if (a > b) return true;
                    break;
                case ConditionWhiteboardIntIntComparison.ComparisonTypes.GREATER_THAN_OR_EQUAL:
                    if (a >= b) return true;
                    break;
                default:
                    return false;
            }
            return false;
        }
        
        public static bool WhiteboardBoolean(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3,
            int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionWhiteboardBoolean vars = (ConditionWhiteboardBoolean)variables;

            bool r = f.fighterWhiteboard.Ints[vars.valueIndex] != 0;
            return vars.inverse ? !r : r;
        }
        
        public static bool ButtonHeld(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionButtonHeld vars = (ConditionButtonHeld)variables;
            
            bool fResult = true;

            for (int i = 0; i < vars.holdTime; i++)
            {
                fResult = f.InputManager.GetButton((int)vars.button, vars.offset+i, 0).isDown;
                if (!fResult) break;
            }
            
            return vars.inverse ? !fResult : fResult;
        }
        
        public static bool NextState(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3,
            int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionNextState vars = (ConditionNextState)variables;

            if (vars.stateMovesetID != -1 && f.FStateManager.nextStateMoveset != vars.stateMovesetID) 
                return vars.inverse ? true : false;
            if (vars.state.GetState() != f.FStateManager.nextState) return vars.inverse ? true : false;
            return !vars.inverse;
        }
        
        public static bool HardKnockdown(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3,
            int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionHardKnockdown vars = (ConditionHardKnockdown)variables;

            bool r = f.FCombatManager.shouldHardKnockdown && f.FCombatManager.hardKnockdownCounter <= FighterCombatManager.MAX_HARDKNOCKDOWNS;
            return vars.inverse ? !r : r;
        }

        public static bool External(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3,
            int arg4)
        {
            FighterManager f = fighter as FighterManager;
            ConditionExternal vars = (ConditionExternal)variables;

            if (!f.FStateManager.TryCondition(f.FStateManager.GetState(), vars.externalCondition.condition,
                    f.FStateManager.CurrentStateFrame)) return false;
            return true;
        }
        
        public static bool StickDir(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3,
            int arg4)
        {
            FighterManager f = fighter as FighterManager;
            var vars = (ConditionStickDir)variables;

            var r = f.FCombatManager.CheckStickDirection(vars.stickDirection, vars.directionDeviation, vars.framesBack);

            return vars.inverse ? !r : r;
        }
        
        public static bool CheckSuccessfulPushblock(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3,
            int arg4)
        {
            FighterManager f = fighter as FighterManager;
            var vars = (ConditionCheckSuccessfulPushblock)variables;

            bool r = (f.Runner.Tick - f.FCombatManager.LastSuccessfulPushblockTick) <= vars.checkLength;
            return vars.inverse ? !r : r;
        }
        
        public static bool HasHitstun(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3,
            int arg4)
        {
            FighterManager f = fighter as FighterManager;
            var vars = (ConditionHasHitstun)variables;

            var r = f.FCombatManager.HitStun > 0;

            return vars.inverse ? !r : r;
        }
        
        public static bool CanBurst(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3,
            int arg4)
        {
            FighterManager f = fighter as FighterManager;
            var vars = (ConditionCanBurst)variables;

            var r = f.FCombatManager.BurstMeter >= FighterCombatManager.MAX_BURST;

            return vars.inverse ? !r : r;
        }
        
        public static bool AuraPercentage(IFighterBase fighter, IConditionVariables variables, HnSF.StateTimeline arg3,
            int arg4)
        {
            FighterManager f = fighter as FighterManager;
            var vars = (ConditionAuraPercentage)variables;

            var r = ((float)f.FCombatManager.Aura / (float)f.fighterDefinition.Aura) >= vars.percentage;

            return vars.inverse ? !r : r;
        }
    }
}