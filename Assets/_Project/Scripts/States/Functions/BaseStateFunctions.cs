using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using HnSF;
using HnSF.Combat;
using HnSF.Fighters;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

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
            if (!vars.overrideStateChange && fm.FStateManager.markedForStateChange) return;
            
            int movesetID = vars.stateMovesetID == -1
                ? fm.FStateManager.CurrentMoveset
                : vars.stateMovesetID;
            int stateID = vars.state.GetState();
            StateTimeline state = (StateTimeline)(fm.FStateManager.GetState(movesetID, stateID));

            if (!vars.ignoreStateConditions && !fm.FStateManager.CheckStateConditions(movesetID, stateID, arg4, 
                    vars.checkInputSequence, vars.checkCondition, 
                    vars.ignoreAirtimeCheck, vars.ignoreStringUseCheck)) return;
            
            if(vars.checkInputSequence) fm.InputManager.ClearBuffer();

            switch (vars.targetType)
            {
                case VarTargetType.Self:
                    fighter.StateManager.MarkForStateChange(vars.state.GetState(), vars.stateMovesetID, vars.frame, vars.overrideStateChange);
                    break;
                case VarTargetType.Throwees:
                    fm.throwees[0].GetBehaviour<FighterManager>().FStateManager.MarkForStateChange(vars.state.GetState(), vars.stateMovesetID, vars.frame, vars.overrideStateChange);
                    break;
            }
        }
        
        public static void ChangeStateList(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager fm = (FighterManager)fighter;
            if (fm.FStateManager.markedForStateChange) return;
            VarChangeStateList vars = (VarChangeStateList)variables;

            for (int i = 0; i < vars.states.Length; i++)
            {
                int movesetID = vars.states[i].movesetID == -1
                    ? fm.FStateManager.CurrentMoveset
                    : vars.states[i].movesetID;
                int stateID = vars.states[i].state.GetState();
                StateTimeline state = (StateTimeline)(fm.FStateManager.GetState(movesetID, stateID));

                if (!fm.FStateManager.CheckStateConditions(movesetID, stateID, arg4, vars.checkInputSequence, vars.checkCondition)) continue;
                if(vars.checkInputSequence) fm.InputManager.ClearBuffer();
                fm.FStateManager.MarkForStateChange(stateID, vars.states[i].movesetID);
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
            
            Vector3 input = vars.inputSource == VarInputSourceType.stick ? f.GetMovementVector(0) : f.myTransform.forward;
            if(vars.normalizeInputSource) input.Normalize();
            if (vars.useRotationIfInputZero && input == Vector3.zero) input = f.myTransform.forward;
            if (vars.reverseInputSource) input = -input;
            f.FPhysicsManager.forceMovement = input * vars.force.GetValue(f);
        }
        
        public static void AddMovement(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarAddMovement vars = (VarAddMovement)variables;
            
            Vector3 input = vars.inputSource == VarInputSourceType.stick ? f.GetMovementVector(0) : f.myTransform.forward;
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
            gravity *= vars.multi.GetValue(f);
            f.FPhysicsManager.HandleGravity(vars.maxFallSpeed.GetValue(f), gravity);
        }
        
        public static void ApplyMovement(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            var vars = (VarApplyMovement)variables;

            float maxSpeed = vars.maxSpeed.GetValue(f);
            if (vars.inputSource == VarInputSourceType.slope)
            {
                maxSpeed *= Mathf.Clamp((Mathf.Clamp(f.groundSlopeAngle, vars.slopeMinClamp, vars.slopeMaxClamp) / vars.slopeDivi) * vars.slopeMulti, 0, vars.slopeMultiMax);
            }

            Vector3 input = Vector3.zero;
            switch (vars.inputSource)
            {
                case VarInputSourceType.stick:
                    input = f.GetMovementVector();
                    break;
                case VarInputSourceType.rotation:
                    input = f.myTransform.forward;
                    break;
                case VarInputSourceType.slope:
                    input = f.groundSlopeDir;
                    input.y = 0;
                    input.Normalize();
                    input += f.GetMovementVector() * vars.slopeinputModi;
                    break;
            }

            if (vars.useSlope)
            {
                var i= f.groundSlopeDir;
                i.y = 0;
                i.Normalize();
                var s = Vector3.Dot(input, i);
                if (s >= 0)
                {
                    maxSpeed *= Mathf.Clamp(
                        (Mathf.Clamp(f.groundSlopeAngle, vars.slopeMinClamp, vars.slopeMaxClamp) / vars.slopeDivi) *
                        vars.slopeMulti, vars.slopeMultiMin, vars.slopeMultiMax);
                }
            }

            f.FPhysicsManager.forceMovement += f.FPhysicsManager.HandleMovement(input, vars.baseAccel.GetValue(f), vars.movementAccel.GetValue(f), 
                vars.deceleration.GetValue(f), vars.minSpeed.GetValue(f), 
                maxSpeed, vars.accelerationFromDot.GetValue(f));
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
        
        public static void ModifyJumpCount(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarModifyJumpCount vars = (VarModifyJumpCount)variables;

            switch (vars.modifyType)
            {
                case VarModifyType.ADD:
                    f.CurrentJump += vars.value;
                    break;
                case VarModifyType.SET:
                    f.CurrentJump = vars.value;
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
                    f.FCombatManager.AddHitStun(vars.applyScaling ? f.FCombatManager.ApplyHitstunScaling(vars.value) : vars.value);
                    break;
                case VarModifyType.SET:
                    f.FCombatManager.SetHitStun(vars.applyScaling ? f.FCombatManager.ApplyHitstunScaling(vars.value) : vars.value);
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
                    f.FStateManager.IncrementFrame(vars.value);
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
                f.FStateManager.ProcessStateVariables((rwby.StateTimeline)arg3, d, arg4, arg3.totalFrames, false);
            }
        }

        public static void ModifyRotation(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            var vars = (VarModifyRotation)variables;

            
            Vector3 wantedDir = Vector3.zero;
            switch (vars.rotateTowards)
            {
                case VarRotateTowardsType.stick:
                    wantedDir = f.GetMovementVector().normalized;
                    break;
                case VarRotateTowardsType.movement:
                    wantedDir = f.FPhysicsManager.forceMovement.normalized;
                    break;
                case VarRotateTowardsType.target:
                    wantedDir = (f.CurrentTarget.transform.position - f.myTransform.position);
                    wantedDir.y = 0;
                    wantedDir.Normalize();
                    break;
                case VarRotateTowardsType.custom:
                    wantedDir = vars.eulerAngle;
                    break;
                case VarRotateTowardsType.wallDir:
                    wantedDir = Vector3.Cross(-f.cWallNormal, Vector3.up) * f.cWallSide;
                    wantedDir.y = 0;
                    wantedDir.Normalize();
                    break;
                case VarRotateTowardsType.camera:
                    wantedDir = f.GetMovementVector(0, 1.0f);
                    break;
            }

            if (wantedDir.sqrMagnitude == 0)
            {
                wantedDir = (vars.useTargetWhenNoMovement && f.CurrentTarget != null) ? 
                    (f.CurrentTarget.transform.position - f.transform.position).normalized 
                    : f.transform.forward;
                wantedDir.y = 0;
                wantedDir.Normalize();
            }
            
            f.SetRotation(wantedDir);
        }
        
        public static void RotateTowards(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            var vars = (VarRotateTowards)variables;
            
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

            if (wantedDir.sqrMagnitude == 0 || f.HardTargeting)
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
                    switch (vars.targetType)
                    {
                        case VarTargetType.Self:
                            f.FPhysicsManager.forceGravity = vars.value.GetValue(f);
                            break;
                        case VarTargetType.Throwees:
                            f.throwees[0].GetBehaviour<FighterManager>().FPhysicsManager.forceGravity = vars.value.GetValue(f);
                            break;
                    }
                    break;
                case VarModifyType.ADD:
                    switch (vars.targetType)
                    {
                        case VarTargetType.Self:
                            f.FPhysicsManager.forceGravity += vars.value.GetValue(f);
                            break;
                        case VarTargetType.Throwees:
                            f.throwees[0].GetBehaviour<FighterManager>().FPhysicsManager.forceGravity += vars.value.GetValue(f);
                            break;
                    }
                    break;
            }
        }

        public static void ModifyEffectSet(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarModifyEffectSet vars = (VarModifyEffectSet)variables;

            Vector3 pBase = Vector3.zero;
            if (vars.OffsetStartAtFighter) pBase = f.myTransform.position;
            switch (vars.modifyType)
            {
                case VarModifyType.SET:
                    f.fighterEffector.SetEffects(vars.wantedEffects);
                    break;
                case VarModifyType.ADD:
                    f.fighterEffector.AddEffects(vars.wantedEffects, pBase, !vars.doNotAddToSet);
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

            fm.projectileManager.CreateProjectile(vars.def, fm.myTransform.position);
            var p = fm.projectileManager.GetLatestProjectile();

            if (vars.useCameraForward)
            {
                Vector3 aimPos = (fm.InputManager.GetCameraPosition() + (fm.InputManager.GetCameraForward() * 100));
                Vector3 cf = (aimPos - fm.myTransform.position).normalized;
                cf *= vars.force.z;
                p.force = cf;
                p.transform.LookAt(aimPos);
                return;
            }
            
            Vector3 f = fm.myTransform.forward * vars.force.z
                        + fm.myTransform.right * vars.force.x
                        + fm.myTransform.up * vars.force.y;
            if ((vars.pointTowardsLockonTargetXZ || vars.pointTowardsLockonTargetY) && fm.CurrentTarget != null)
            {
                Vector3 dir = (fm.CurrentTarget.transform.position - fm.myTransform.position).normalized;
                
                if (vars.pointTowardsLockonTargetXZ)
                {
                    // TODO: x force
                    f.x = 0;
                    f.z = 0;
                    var dirNoY = dir;
                    if(!vars.pointTowardsLockonTargetY) dirNoY.y = 0;
                    f += dirNoY * vars.force.z;
                }
                p.transform.LookAt(fm.CurrentTarget.transform.position);
            }
            p.force = f;
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
        
        public static void LogMessage(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager fm = (FighterManager)fighter;
            VarDebugLog vars = (VarDebugLog)variables;
            
            Debug.Log(vars.message);
        }
        
        public static void FindWall(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            bool IsHitValid(FighterManager fm, VarFindWall vars, RaycastHit hitResult, Vector3 inputAngle)
            {
                float angle = Vector3.SignedAngle(inputAngle, -hitResult.normal, Vector3.up);
                return angle >= vars.minAngle && angle <= vars.maxAngle;
            }
            
            FighterManager fm = (FighterManager)fighter;
            var vars = (VarFindWall)variables;
            
            if(vars.clearWallIfNotFound) fm.ClearWall();
            
            Vector3 input = vars.inputSource == VarInputSourceType.stick ? fm.GetMovementVector(0) : fm.myTransform.forward;
            if(vars.normalizeInputSource) input.Normalize();
            if (input == Vector3.zero)
            {
                if (!vars.useRotationIfInputZero) return;
                    input = fm.transform.forward;
            }

            input = Quaternion.Euler(0, vars.inputSourceOffset, 0) * input;

            for (int i = 0; i < fm.wallHitResults.Length; i++)
            {
                fm.wallHitResults[i] = new RaycastHit();
            }

            var distance = fm.FPhysicsManager.ecbRadius + 0.2f + vars.rayDistance;
            float angle = vars.angleBasedOnWallDir ? (fm.cWallSide == 1 ? 90 : -90) : vars.startAngleOffset;
            for (int i = 0; i < vars.raycastCount; i++)
            {
                float x = Mathf.Sin (angle);
                float z = Mathf.Cos (angle);
                angle += 2 * Mathf.PI / vars.raycastCount;

                Vector3 dir = fm.myTransform.right * x
                              + fm.myTransform.forward * z;
                fm.Runner.GetPhysicsScene().Raycast(fm.GetCenter(), dir.normalized, out fm.wallHitResults[i],
                    distance, fm.wallLayerMask, QueryTriggerInteraction.Ignore);
            }

            float min = Mathf.Infinity;
            int lowestIndex = -1;
            for (int i = 0; i < fm.wallHitResults.Length; i++)
            {
                if (fm.wallHitResults[i].point == Vector3.zero) continue;
                if (IsHitValid(fm, vars, fm.wallHitResults[i], input) && fm.wallHitResults[i].distance < min)
                {
                    lowestIndex = i;
                    min = fm.wallHitResults[i].distance;
                }
            }

            if (lowestIndex == -1) return;

            fm.AssignWall(input, fm.wallHitResults[lowestIndex]);
        }
        
        public static void FindPole(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager fm = (FighterManager)fighter;
            VarFindPole vars = (VarFindPole)variables;
            
            Vector3 bottomPoint = fm.myTransform.position + fm.FPhysicsManager.cc.center + Vector3.up * -fm.FPhysicsManager.cc.height * 0.5F;
            Vector3 topPoint = bottomPoint + Vector3.up * fm.FPhysicsManager.cc.height;
            
            int hits = fm.Runner.GetPhysicsScene().OverlapCapsule(bottomPoint, topPoint, fm.FPhysicsManager.cc.radius,
                fm.colliderBuffer, fm.poleLayerMask, QueryTriggerInteraction.UseGlobal);

            if (hits == 0)
            {
                fm.foundPole = null;
                return;
            }

            fm.foundPole = fm.colliderBuffer[0].gameObject.GetComponent<Pole>();
        }
        
        public static void SnapToWall(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager fm = (FighterManager)fighter;
            var vars = (VarSnapToWall)variables;

            if (!fm.IsWallValid()) return;

            Vector3 newPos = fm.cWallPoint + (fm.cWallNormal * fm.FPhysicsManager.ecbRadius) - (new Vector3(0, fm.FPhysicsManager.ecbOffset, 0));

            fm.FPhysicsManager.SetPosition(newPos, false);
            var tempNormal = fm.cWallNormal;
            tempNormal.y = 0;
            tempNormal.Normalize();
            if (vars.useWallSide)
            {
                fm.SetRotation(Vector3.Cross(-tempNormal, Vector3.up) * fm.cWallSide, false);
            }
            else
            {
                fm.SetRotation(-tempNormal, false);
            }
        }
        
        public static void SnapToPole(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager fm = (FighterManager)fighter;
            VarSnapToPole vars = (VarSnapToPole)variables;

            if (fm.foundPole == null) return;
            fm.FPhysicsManager.SetPosition(fm.foundPole.GetNearestPoint(fm.transform.position), false);
            fm.SetRotation(fm.foundPole.GetNearestFaceDirection(fm.transform.forward));
            fm.poleMagnitude = fm.poleSpinLaunchForce; //(fm.FPhysicsManager.forceMovement + new Vector3(0, fm.FPhysicsManager.forceGravity, 0)).magnitude;
        }
        
        public static void ClampMovement(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager fm = (FighterManager)fighter;
            VarClampMovement vars = (VarClampMovement)variables;

            fm.FPhysicsManager.forceMovement = Vector3.ClampMagnitude(fm.FPhysicsManager.forceMovement, vars.magnitude.GetValue(fm));
        }
        
        public static void TeleportRaycast(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager fm = (FighterManager)fighter;
            var vars = (VarTeleportRaycast)variables;

            if ((vars.startPoint == VarTeleportRaycast.StartPointTypes.Target || vars.raycastDirectionSource ==
                    VarTeleportRaycast.RaycastDirSource.TARGET_VECTOR)
                && fm.CurrentTarget == null)
            {
                return;
            }
            
            Vector3 dir = Vector3.zero;
            switch (vars.raycastDirectionSource)
            {
                case VarTeleportRaycast.RaycastDirSource.STICK:
                    break;
                case VarTeleportRaycast.RaycastDirSource.ROTATION:
                    dir = fm.myTransform.forward * vars.direction.z
                          + fm.myTransform.right * vars.direction.x
                          + fm.myTransform.up * vars.direction.y;
                    break;
                case VarTeleportRaycast.RaycastDirSource.TARGET_VECTOR:
                    var fr = (fm.CurrentTarget.transform.position - fm.myTransform.position);
                    fr.y = 0;
                    fr.Normalize();
                    var rght = -Vector3.Cross(fr, Vector3.up);
                    dir = fr * vars.direction.z 
                          + rght * vars.direction.x 
                          + Vector3.up * vars.direction.y;
                    break;
                case VarTeleportRaycast.RaycastDirSource.CUSTOM:
                    dir = fm.GetMovementVector(vars.direction.x, vars.direction.z);
                    dir.y = vars.direction.y;
                    break;
            }
            dir.Normalize();

            Vector3 startPos = fm.myTransform.position;
            if (vars.startPoint == VarTeleportRaycast.StartPointTypes.Target)
            {
                startPos = fm.CurrentTarget.transform.position;
            }
            
            Vector3 bottomPoint = startPos + fm.FPhysicsManager.cc.center + Vector3.up * -fm.FPhysicsManager.cc.height * 0.5F;
            Vector3 topPoint = bottomPoint + Vector3.up * fm.FPhysicsManager.cc.height;

            bool hit = fm.Runner.GetPhysicsScene().CapsuleCast(bottomPoint + (Vector3.up*vars.startUpOffset), 
                topPoint + (Vector3.up*vars.startUpOffset), 
                fm.FPhysicsManager.cc.radius * 0.9f, dir, out fm.wallHitResults[0], 
                vars.distance, fm.wallLayerMask.value);

            if (hit)
            {
                Vector3 newPos = fm.transform.position + (fm.wallHitResults[0].distance) * dir;
                fm.FPhysicsManager.ForceUnground();
                fm.FPhysicsManager.SetPosition(newPos, vars.bypassInterpolation);
                //Vector3 newPos = fm.wallHitResults[0].point 
                //                 + (new Vector3(fm.wallHitResults[0].normal.x, 0, fm.wallHitResults[0].normal.z) * fm.FPhysicsManager.cc.radius)
                //                 - (new Vector3(0, fm.wallHitResults[0].normal.y, 0) * (bottomPoint - fm.myTransform.position).y );
                //fm.FPhysicsManager.SetPosition(newPos, vars.bypassInterpolation);
            }
            else
            {
                if (!vars.goToPosOnNoHit) return;
                Vector3 newPos = startPos + (dir * vars.distance);
                fm.FPhysicsManager.SetPosition(newPos, vars.bypassInterpolation);
            }
        }
        
        public static void ModifyMoveset(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager fm = (FighterManager)fighter;
            VarModifyMoveset vars = (VarModifyMoveset)variables;

            fm.StateManager.SetMoveset(vars.modifyType == VarModifyType.SET ? vars.value : fm.StateManager.CurrentStateMoveset + vars.value);
        }
        
        public static void ModifyPoleAngle(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            VarModifyPoleAngle vars = (VarModifyPoleAngle)variables;

            switch (vars.modifyType)
            {
                case VarModifyType.ADD:
                    fm.poleSpin += vars.value;
                    Vector3 rotatedVector = Quaternion.Euler(fm.poleSpin, 0, 0) * Vector3.forward;
                    //Debug.DrawRay(fm.myTransform.position, (fm.myTransform.forward * rotatedVector.z) + (fm.myTransform.right * rotatedVector.x)  + (Vector3.up * rotatedVector.y), Color.red, fm.Runner.DeltaTime);
                    break;
                case VarModifyType.SET:
                    
                    break;
            }
        }
        
        public static void TransferPoleMomentum(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            VarTransferPoleMomentum vars = (VarTransferPoleMomentum)variables;

            Vector3 rotatedVector = Quaternion.Euler(fm.poleSpin, 0, 0) * Vector3.forward;
            rotatedVector *= fm.poleMagnitude;

            fm.FPhysicsManager.forceGravity = rotatedVector.y;
            rotatedVector.y = 0;
            fm.FPhysicsManager.forceMovement = (fm.myTransform.forward * rotatedVector.z) + (fm.myTransform.right * rotatedVector.x);

            fm.poleMagnitude = 0;
            fm.poleSpin = 0;
        }
        
        public static void ClearThrowee(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;

            for (int i = 0; i < fm.throwees.Length; i++)
            {
                fm.throwees.Set(i, null);
            }
        }
        
        public static void ModifyBlockstun(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            VarModifyBlockstun vars = (VarModifyBlockstun)variables;

            switch (vars.modifyType)
            {
                case VarModifyType.ADD:
                    fm.FCombatManager.BlockStun += vars.value;
                    break;
                case VarModifyType.SET:
                    fm.FCombatManager.BlockStun = vars.value;
                    break;
            }

            fm.FCombatManager.BlockStun = Mathf.Clamp(fm.FCombatManager.BlockStun, 0, int.MaxValue);
        }
        
        public static void SetGuardState(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            VarSetGuardState vars = (VarSetGuardState)variables;

            fm.FCombatManager.BlockState = vars.state;
        }
        
        public static void ModifyAura(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            VarModifyAura vars = (VarModifyAura)variables;

            switch (vars.modifyType)
            {
                case VarModifyType.ADD:
                    fm.FCombatManager.AddAura(vars.value.GetValue(fm));
                    break;
                case VarModifyType.SET:
                    fm.FCombatManager.SetAura(vars.value.GetValue(fm));
                    break;
            }
        }
        
        public static void SetBlockState(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            VarSetBlockState vars = (VarSetBlockState)variables;

            fm.FCombatManager.BlockState = vars.state;
        }
        
        public static void ClearCurrentEffects(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            var vars = (VarClearCurrentEffects)variables;

            fm.fighterEffector.ClearCurrentEffects(!vars.keepEffects, vars.autoIncrementEffects);
        }
        
        public static void ModifySoundSet(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            VarModifySoundSet vars = (VarModifySoundSet)variables;
            
            fm.fighterSounder.AddSFXs(vars.sounds);
        }
        
        public static void FootstepSFX(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;

            if (!fm.Runner.GetPhysicsScene().Raycast(fm.myTransform.position + Vector3.up, Vector3.down, out var hit,
                    5.0f, fm.wallLayerMask)) return;

                Renderer rend = hit.transform.GetComponent<Renderer>();
            MeshCollider meshCollider = hit.collider as MeshCollider;

            if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null ||
                meshCollider == null || rend.sharedMaterial.HasTexture(ExtDebug.MATERIAL_TEXTURE_IDENTIFIER) == false)
            {
                return;
            }
            
            Texture2D tex = rend.material.GetTexture(ExtDebug.MATERIAL_TEXTURE_IDENTIFIER) as Texture2D;
            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= tex.width;
            pixelUV.y *= tex.height;

            var c = tex.GetPixel((int)pixelUV.x, (int)pixelUV.y);
            var temp = fm.fighterSounder.rng;
            if (c == Color.red)
            {
                fm.fighterSounder.AddSFX(fm.footstepsWood[temp.RangeExclusive(0, fm.footstepsWood.Length)]);
            }else if (c == Color.blue)
            {
                fm.fighterSounder.AddSFX(fm.footstepsSnow[temp.RangeExclusive(0, fm.footstepsSnow.Length)]);
            }else if (c == Color.green)
            {
                fm.fighterSounder.AddSFX(fm.footstepsLeaves[temp.RangeExclusive(0, fm.footstepsLeaves.Length)]);
            }
            else
            {
                fm.fighterSounder.AddSFX(fm.footstepsDirt[temp.RangeExclusive(0, fm.footstepsDirt.Length)]);
            }
            fm.fighterSounder.rng = temp;
        }
        
        public static void IncrementChargeLevel(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            VarIncrementChargeLevel vars = (VarIncrementChargeLevel)variables;
            
            bool fResult = false;

            var buttonInt = (int)vars.button;
            if (vars.checkAbilityButton)
            {
                switch (fm.FCombatManager.lastUsedSpecial)
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
            
            var b = fm.InputManager.GetButton(buttonInt, 0, 0);
            
            if (!b.isDown
                || (fm.FCombatManager.CurrentChargeLevel == vars.maxLevel && !vars.canHold))
            {
                fm.FStateManager.IncrementFrame(amt: 1);
                return;
            }

            if (fm.CombatManager.CurrentChargeLevel == vars.maxLevel) return;
            
            fm.FCombatManager.IncrementChargeLevelCharge(vars.chargePerLevel);
        }
        
        public static void ModifyCameraMode(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            VarModifyCameraMode vars = (VarModifyCameraMode)variables;

            switch (vars.modifyType)
            {
                case VarModifyType.ADD:
                    fm.cameraMode += vars.value;
                    break;
                case VarModifyType.SET:
                    fm.cameraMode = vars.value;
                    break;
            }
        }
        
        public static void ModifyAttackStringList(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            VarModifyAttackStringList vars = (VarModifyAttackStringList)variables;

            switch (vars.actionType)
            {
                case VarModifyAttackStringList.StringListActionTypes.CLEAR:
                    fm.FCombatManager.ResetString();
                    break;
            }
        }
        
        public static void SetCounterhitState(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;

            fm.FCombatManager.CounterhitState = true;
        }
        
        public static void SetPushblockState(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            VarSetPushblockState vars = (VarSetPushblockState)variables;

            switch (vars.pushblockState)
            {
                case PushblockState.PERFECT:
                    fm.FCombatManager.CurrentPushblockState = PushblockState.PERFECT;
                    break;
                case PushblockState.GUARD:
                    if (fm.FCombatManager.LastPushblockAttempt != 0 &&
                        (fm.Runner.Tick - fm.FCombatManager.LastPushblockAttempt) < 30) return;
                    fm.FCombatManager.CurrentPushblockState = PushblockState.GUARD;
                    fm.FCombatManager.LastPushblockAttempt = fm.Runner.Tick;
                    break;
            }
        }
        
        public static void ConsumeWallBounce(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            VarConsumeWallBounce vars = (VarConsumeWallBounce)variables;

            fm.FCombatManager.CurrentWallBounces++;
            fm.FCombatManager.WallBounce = false;
            var f = fm.cWallNormal * fm.FCombatManager.WallBounceForce;
            fm.FPhysicsManager.forceGravity = f.y;
            f.y = 0;
            fm.FPhysicsManager.forceMovement = f;
        }
        
        public static void ConsumeGroundBounce(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            VarConsumeGroundBounce vars = (VarConsumeGroundBounce)variables;

            fm.FCombatManager.CurrentGroundBounces++;
            fm.FCombatManager.GroundBounce = false;
            fm.FPhysicsManager.forceGravity = fm.FCombatManager.GroundBounceForce;
            fm.FPhysicsManager.forceMovement = Vector3.zero;
        }
        
        public static void MoveTowardsMagnitude(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            VarMoveTowardsMagnitude vars = (VarMoveTowardsMagnitude)variables;

            Vector3 m = fm.FPhysicsManager.GetOverallForce();
            if (vars.applyGravity && vars.applyMovement)
            {
                m = fm.FPhysicsManager.GetOverallForce();
            }else if (vars.applyGravity)
            {
                m = new Vector3(0, fm.FPhysicsManager.forceGravity, 0);
            }
            else
            {
                m = fm.FPhysicsManager.forceMovement;
            }
            m = Vector3.MoveTowards(m, m.normalized * vars.force.GetValue(fm), vars.distance.GetValue(fm));
            if(vars.applyGravity) fm.FPhysicsManager.forceGravity = m.y;
            m.y = 0;
            if(vars.applyMovement) fm.FPhysicsManager.forceMovement = m;
        }
        
        public static void SetGroundedState(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            VarSetGroundedState vars = (VarSetGroundedState)variables;

            fm.FStateManager.CurrentGroundedState = vars.groundedGroup;
        }
        
        public static void ModifyIntWhiteboard(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            VarModifyIntWhiteboard vars = (VarModifyIntWhiteboard)variables;
            
            fm.fighterWhiteboard.UpdateInt(vars.index, vars.modifyType, vars.val);
        }

        public static void ProjectilePointToTarget(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            VarProjectilePointToTarget vars = (VarProjectilePointToTarget)variables;

            if (!fm.CurrentTarget) return;

            var p = fm.projectileManager.GetLatestProjectile(vars.projectileOffset);
            
            var dir = fm.CurrentTarget.GetComponent<ITargetable>().GetBounds().center - p.transform.position;
            
            p.SetRotation(Quaternion.LookRotation(dir.normalized));
        }

        public static void ProjectileModifyForce(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            VarProjectileModifyForce vars = (VarProjectileModifyForce)variables;
            
            var p = fm.projectileManager.GetLatestProjectile(vars.projectileOffset);

            if (vars.keepRawForce)
            {
                p.force = vars.force;
                return;
            }
            p.force = p.transform.forward * vars.force.z
                      + p.transform.right * vars.force.x
                      + p.transform.up * vars.force.y;
        }
        
        public static void SetProjectileTarget(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            VarSetProjectileTarget vars = (VarSetProjectileTarget)variables;
            
            var p = fm.projectileManager.GetLatestProjectile(vars.projectileOffset);

            ((ITargetingProjectile)p).Target = fm.CurrentTarget;
        }

        public static void ModifyProjectileHomingStrength(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            var vars = (VarProjectileModifyHomingStrength)variables;
            
            var p = fm.projectileManager.GetLatestProjectile(vars.projectileOffset);

            ((IHomingProjectile)p).AutoTargetStrength = vars.homingStrength;
        }

        public static void DirectDamage(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            var vars = (VarDirectDamage)variables;

            var hurtInfo = new HurtInfo(vars.hitboxInfo, -1,
                fm.transform.position, fm.transform.forward, fm.transform.right,
                fm.FPhysicsManager.GetOverallForce(), Vector3.zero);
            
            switch (vars.targetType)
            {
                case VarTargetType.Throwees:
                    var throwee = fm.throwees[0];
                    var hurtable = throwee.GetComponent<IHurtable>();
                    hurtInfo.hitPosition = throwee.transform.position;
                    HitReaction reaction = (HitReaction)hurtable.Hurt(hurtInfo);
                    fm.FCombatManager.HitboxManager.HandleDirectHitReaction(vars.hitboxInfo, reaction);
                    break;
            }
        }
        
        public static void ClearBuffer(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            
            fm.InputManager.ClearBuffer();
        }
        
        public static void ModifyProjectileRotation(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;
            var vars = (VarProjectileSetRotation)variables;
            
            var p = fm.projectileManager.GetLatestProjectile(vars.projectileOffset);

            Vector3 rot = fm.myTransform.eulerAngles;
            switch (vars.rotationSource)
            {
                case VarInputSourceType.rotation:
                    break;
                case VarInputSourceType.custom:
                    break;
            }

            rot += vars.rotation;
            
            p.SetRotation(rot);
        }
        
        public static void ConsumeHardKnockdown(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline stateTimeline, int frame)
        {
            FighterManager fm = (FighterManager)fighter;

            fm.FCombatManager.hardKnockdownCounter++;
            if (fm.FCombatManager.hardKnockdownCounter > FighterCombatManager.MAX_HARDKNOCKDOWNS)
                fm.FCombatManager.shouldHardKnockdown = false;
        }
    }
}