using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using HnSF;
using HnSF.Fighters;
using UnityEngine;

namespace rwby
{
    public static class BaseStateFunctions
    {
        public static void Null(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            
        }
        
        public static void TrySpecial(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager fm = (FighterManager)fighter;
            VarTrySpecial vars = (VarTrySpecial)variables;

            fm.FCombatManager.TrySpecial();
        }
        
        public static void ChangeState(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager fm = (FighterManager)fighter;
            VarChangeState vars = (VarChangeState)variables;
            
            int movesetID = vars.stateMovesetID == -1
                ? fm.FStateManager.CurrentStateMoveset
                : vars.stateMovesetID;
            int stateID = vars.state.GetState();
            StateTimeline state = (StateTimeline)(fm.FStateManager.GetState(movesetID, stateID));

            //if (!fm.FCombatManager.MovePossible(new MovesetStateIdentifier(movesetID, stateID), state.maxUsesInString)) return;
            if (vars.checkInputSequence && !fm.FCombatManager.CheckForInputSequence(state.inputSequence)) return;
            if (vars.checkCondition && !fm.FStateManager.TryCondition(state, state.condition, arg4)) return;
            
            fighter.StateManager.MarkForStateChange(vars.state.GetState(), vars.stateMovesetID);
        }
        
        public static void ChangeStateList(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager fm = (FighterManager)fighter;
            VarChangeStateList vars = (VarChangeStateList)variables;

            for (int i = 0; i < vars.states.Length; i++)
            {
                int movesetID = vars.states[i].movesetID == -1
                    ? fm.FStateManager.CurrentStateMoveset
                    : vars.states[i].movesetID;
                int stateID = vars.states[i].state.GetState();
                StateTimeline state = (StateTimeline)(fm.FStateManager.GetState(movesetID, stateID));

                if (!fm.FCombatManager.MovePossible(new MovesetStateIdentifier(movesetID, stateID), state.maxUsesInString)) continue;
                if (vars.checkInputSequence && !fm.FCombatManager.CheckForInputSequence(state.inputSequence)) continue;
                if (vars.checkCondition && !fm.FStateManager.TryCondition(state, state.condition, arg4)) continue;

                fm.FStateManager.ChangeState(stateID, vars.states[i].movesetID);
                return;
            }
        }
        
        public static void ApplyTraction(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            VarApplyTraction vars = (VarApplyTraction)variables;
            FighterManager fm = (FighterManager)fighter;
            float t = vars.traction.GetValue(fm);
            if (vars.applyMovement)
            {
                fm.FPhysicsManager.ApplyMovementFriction(t);
            }

            if (vars.applyGravity)
            {
                fm.FPhysicsManager.ApplyGravityFriction(t);
            }
        }

        public static void SetMovement(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarSetMovement vars = (VarSetMovement)variables;
            
            Vector3 input = vars.inputSource == VarSetMovement.InputSource.stick ? f.GetMovementVector(0) : f.myTransform.forward;
            if(vars.normalizeInputSource) input.Normalize();
            if (vars.useRotationIfInputZero && input == Vector3.zero) input = f.myTransform.forward;
            f.FPhysicsManager.forceMovement = input * vars.force.GetValue(f);
        }
        
        public static void AddMovement(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarAddMovement vars = (VarAddMovement)variables;
            
            Vector3 input = vars.inputSource == VarSetMovement.InputSource.stick ? f.GetMovementVector(0) : f.myTransform.forward;
            if(vars.normalizeInputSource) input.Normalize();
            if (vars.useRotationIfInputZero && input == Vector3.zero) input = f.myTransform.forward;
            f.FPhysicsManager.forceMovement += input * vars.force.GetValue(f);
        }

        public static void SetFallSpeed(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarSetFallSpeed vars = (VarSetFallSpeed)variables;

            f.FPhysicsManager.forceGravity = vars.value;
        }

        public static void ApplyGravity(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarApplyGravity vars = (VarApplyGravity)variables;

            float gravity = vars.useValue ? vars.value.GetValue(f) : (2 * vars.jumpHeight.GetValue(f)) / Mathf.Pow(vars.jumpTime.GetValue(f) / 2.0f, 2);
            f.FPhysicsManager.HandleGravity(vars.maxFallSpeed.GetValue(f), gravity, vars.multi.GetValue(f));
        }
        
        public static void ApplyMovement(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarApplyMovement vars = (VarApplyMovement)variables;

            f.FPhysicsManager.forceMovement += f.FPhysicsManager.HandleMovement(vars.baseAccel.GetValue(f), vars.movementAccel.GetValue(f), 
                vars.deceleration.GetValue(f), vars.minSpeed.GetValue(f), 
                vars.maxSpeed.GetValue(f), vars.accelerationFromDot.GetValue(f));
        }

        public static void ModifyAirDashCount(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarModifyAirDashCount vars = (VarModifyAirDashCount)variables;

            switch (vars.modifyType)
            {
                case VarModifyType.ADD:
                    f.CurrentAirDash += vars.value;
                    break;
                case VarModifyType.SET:
                    f.CurrentAirDash = vars.value;
                    break;
            }
        }

        public static void ModifyHitstun(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarModifyHitstun vars = (VarModifyHitstun)variables;
            
            switch (vars.modifyType)
            {
                case VarModifyType.ADD:
                    f.FCombatManager.AddHitStun(vars.value);
                    break;
                case VarModifyType.SET:
                    f.FCombatManager.SetHitStun(vars.value);
                    break;
            }
        }

        public static void SetECB(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarSetECB vars = (VarSetECB)variables;
            
            f.FPhysicsManager.SetECB(vars.ecbCenter, vars.ecbRadius, vars.ecbHeight);
        }
        
        public static void SnapECB(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            f.FPhysicsManager.SnapECB();
        }

        public static void ModifyFrame(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarModifyFrame vars = (VarModifyFrame)variables;

            switch (vars.modifyType)
            {
                case VarModifyType.ADD:
                    f.StateManager.IncrementFrame(vars.value);
                    break;
                case VarModifyType.SET:
                    f.StateManager.SetFrame(vars.value);
                    break;
            }
        }

        public static void ApplyJumpForce(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarApplyJumpForce vars = (VarApplyJumpForce)variables;

            if (vars.useValue)
            {
                f.FPhysicsManager.forceGravity = vars.value;
            }
            else
            {
                float apexTime = vars.maxJumpTime.GetValue(f) / 2.0f;
                f.FPhysicsManager.forceGravity = (2.0f * vars.jumpHeight.GetValue(f)) / apexTime;   
            }
        }

        public static void External(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarExternal vars = (VarExternal)variables;

            foreach (var d in vars.asset.data)
            {
                f.FStateManager.ProcessStateVariables((rwby.StateTimeline)arg3, d, arg4, false);
            }
        }

        public static void ModifyRotation(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarModifyRotation vars = (VarModifyRotation)variables;

            
            Vector3 wantedDir = Vector3.zero;
            switch (vars.rotateTowards)
            {
                case VarRotateTowardsType.stick:
                    wantedDir = f.GetMovementVector().normalized;
                    break;
                case VarRotateTowardsType.movement:
                    wantedDir = f.FPhysicsManager.forceMovement.normalized;
                    break;
                case VarRotateTowardsType.custom:
                    wantedDir = vars.eulerAngle;
                    break;
            }
            if (wantedDir.sqrMagnitude == 0) wantedDir = f.transform.forward;
            
            f.SetVisualRotation(wantedDir);
        }
        
        public static void RotateTowards(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarRotateTowards vars = (VarRotateTowards)variables;
            
            Vector3 wantedDir = Vector3.zero;
            switch (vars.rotateTowards)
            {
                case VarRotateTowardsType.stick:
                    wantedDir = f.GetMovementVector();
                    break;
                case VarRotateTowardsType.movement:
                    wantedDir = f.FPhysicsManager.forceMovement;
                    break;
                case VarRotateTowardsType.target:
                    if (f.CurrentTarget != null)
                    {
                        wantedDir = f.CurrentTarget.transform.position - f.myTransform.position;
                        wantedDir.y = 0;
                        wantedDir.Normalize();
                    }
                    break;
                case VarRotateTowardsType.custom:
                    wantedDir = vars.eulerAngle;
                    break;
            }

            if (wantedDir.sqrMagnitude == 0)
            {
                if (!vars.rotateTowardsTarget || f.CurrentTarget == null) return;
                wantedDir = f.CurrentTarget.transform.position - f.myTransform.position;
                wantedDir.y = 0;
                wantedDir.Normalize();
            }
            
            f.RotateTowards(wantedDir, vars.rotationSpeed.GetValue(f));
        }

        public static void ModifyAnimationSet(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3,
            int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarModifyAnimationSet vars = (VarModifyAnimationSet)variables;

            switch (vars.modifyType)
            {
                case VarModifyType.SET:
                    f.fighterAnimator.SetAnimationSet(0, vars.wantedAnimations, vars.fadeTime);
                    break;
                case VarModifyType.ADD:
                    f.fighterAnimator.AddAnimationToSet(0, vars.wantedAnimations);
                    break;
            }
        }
        
        public static void ModifyAnimationFrame(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3,
            int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarModifyAnimationFrame vars = (VarModifyAnimationFrame)variables;

            switch (vars.modifyType)
            {
                case VarModifyType.SET:
                    f.fighterAnimator.SetAnimationTime(0, vars.animations, vars.frame);
                    break;
                case VarModifyType.ADD:
                    f.fighterAnimator.AddAnimationTime(0, vars.animations, vars.frame);
                    break;
            }
        }
        
        public static void ModifyAnimationWeight(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3,
            int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarModifyAnimationWeight vars = (VarModifyAnimationWeight)variables;

            switch (vars.modifyType)
            {
                case VarModifyType.SET:
                    f.fighterAnimator.SetAnimationWeight(0, vars.animations, vars.weight);
                    break;
                case VarModifyType.ADD:
                    f.fighterAnimator.AddAnimationWeight(0, vars.animations, vars.weight);
                    break;
            }
        }

        public static void CreateBox(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarCreateBox vars = (VarCreateBox)variables;
            
            f.BoxManager.AddBox(vars.boxType, vars.attachedTo, vars.shape, vars.offset, vars.boxExtents, vars.radius, vars.definitionIndex, (StateTimeline)arg3);
        }

        public static void MultiplyMovement(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarMultiplyMovement vars = (VarMultiplyMovement)variables;

            f.FPhysicsManager.forceMovement *= vars.multiplier.GetValue(f);
        }

        public static void ClampGravity(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarClampGravity vars = (VarClampGravity)variables;

            f.FPhysicsManager.forceGravity = Mathf.Clamp(f.FPhysicsManager.forceGravity, vars.minValue.GetValue(f), vars.maxValue.GetValue(f));
        }

        public static void MultiplyGravity(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarMultiplyGravity vars = (VarMultiplyGravity)variables;

            f.FPhysicsManager.forceGravity *= vars.multiplier.GetValue(f);
        }

        public static void ModifyFallSpeed(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarModifyFallSpeed vars = (VarModifyFallSpeed)variables;

            switch (vars.modifyType)
            {
                case VarModifyType.SET:
                    f.FPhysicsManager.forceGravity = vars.value.GetValue(f);
                    break;
                case VarModifyType.ADD:
                    f.FPhysicsManager.forceGravity += vars.value.GetValue(f);
                    break;
            }
        }

        public static void ModifyEffectSet(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarModifyEffectSet vars = (VarModifyEffectSet)variables;

            switch (vars.modifyType)
            {
                case VarModifyType.SET:
                    f.fighterEffector.SetEffects(vars.wantedEffects);
                    break;
                case VarModifyType.ADD:
                    f.fighterEffector.AddEffects(vars.wantedEffects);
                    break;
            }
        }

        public static void ModifyEffectFrame(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarModifyEffectFrame vars = (VarModifyEffectFrame)variables;

            switch (vars.modifyType)
            {
                case VarModifyType.SET:
                    f.fighterEffector.SetEffectTime(vars.effects, vars.frame);
                    break;
                case VarModifyType.ADD:
                    f.fighterEffector.AddEffectTime(vars.effects, vars.frame);
                    break;
            }
        }
        
        public static void ModifyEffectRotation(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarModifyEffectRotation vars = (VarModifyEffectRotation)variables;

            switch (vars.modifyType)
            {
                case VarModifyType.SET:
                    f.fighterEffector.SetEffectRotation(vars.effects, vars.rotation);
                    break;
                case VarModifyType.ADD:
                    f.fighterEffector.AddEffectRotation(vars.effects, vars.rotation);
                    break;
            }
        }
        
        public static void CreateProjectile(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager fm = (FighterManager)fighter;
            VarCreateProjectile vars = (VarCreateProjectile)variables;

            var pos = fm.myTransform.position;
            pos += fm.myTransform.forward * vars.positionOffset.z;
            pos += fm.myTransform.right * vars.positionOffset.x;
            pos += fm.myTransform.up * vars.positionOffset.y;
            
            var predictionKey = new NetworkObjectPredictionKey {Byte0 = (byte) fm.Runner.Simulation.Tick, Byte1 = (byte)fm.Object.InputAuthority.PlayerId};
            
            fm.Runner.Spawn(vars.projectile, pos, Quaternion.Euler(fm.myTransform.eulerAngles + vars.rotation), fm.Object.InputAuthority,
                (a, b) =>
                {
                    b.GetComponent<ProjectileBase>().owner = fm.Object;
                    b.GetComponent<ProjectileBase>().team = fm.FCombatManager.Team;
                }, 
                predictionKey);
        }
        
        public static void ClearHitList(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager fm = (FighterManager)fighter;
            VarClearHitList vars = (VarClearHitList)variables;

            if (vars.frameDivider == 0 || fm.FStateManager.CurrentStateFrame % vars.frameDivider == 0)
            {
                fm.FCombatManager.HitboxManager.Reset();
            }
        }
        
        public static void FindSoftTarget(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager fm = (FighterManager)fighter;
            if (fm.HardTargeting) return;
            VarFindSoftTarget vars = (VarFindSoftTarget)variables;
            
            fm.CurrentTarget = null;
            Vector2 movementDir = fm.InputManager.GetMovement(0);
            if (movementDir.sqrMagnitude <= InputConstants.movementDeadzone * InputConstants.movementDeadzone)
            {
                fm.PickLockonTarget(fm.lockonMaxDistance);
            }
        }
    }
}