using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using HnSF;
using HnSF.Fighters;
using UnityEngine;
using UnityEngine.Profiling;

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
            if (fm.FStateManager.markedForStateChange) return;
            VarChangeState vars = (VarChangeState)variables;
            
            int movesetID = vars.stateMovesetID == -1
                ? fm.FStateManager.CurrentMoveset
                : vars.stateMovesetID;
            int stateID = vars.state.GetState();
            StateTimeline state = (StateTimeline)(fm.FStateManager.GetState(movesetID, stateID));

            //if (!fm.FCombatManager.MovePossible(new MovesetStateIdentifier(movesetID, stateID), state.maxUsesInString)) return;
            if (vars.checkInputSequence && !fm.FCombatManager.CheckForInputSequence(state.inputSequence, holdInput: state.inputSequenceAsHoldInputs)) return;
            if (vars.checkCondition && !fm.FStateManager.TryCondition(state, state.condition, arg4)) return;

            switch (vars.targetType)
            {
                case VarTargetType.Self:
                    fighter.StateManager.MarkForStateChange(vars.state.GetState(), vars.stateMovesetID);
                    break;
                case VarTargetType.Throwees:
                    fm.throwees[0].GetBehaviour<FighterManager>().FStateManager.MarkForStateChange(vars.state.GetState(), vars.stateMovesetID);
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

                if (!fm.FCombatManager.MovePossible(new MovesetStateIdentifier(movesetID, stateID), state.maxUsesInString)) continue;
                if (vars.checkInputSequence && !fm.FCombatManager.CheckForInputSequence(state.inputSequence, holdInput: state.inputSequenceAsHoldInputs)) continue;
                if (vars.checkCondition && !fm.FStateManager.TryCondition(state, state.condition, arg4)) continue;

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
            f.FPhysicsManager.HandleGravity(vars.maxFallSpeed.GetValue(f), gravity, vars.multi.GetValue(f));
        }
        
        public static void ApplyMovement(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager f = (FighterManager)fighter;
            VarApplyMovement vars = (VarApplyMovement)variables;

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
                f.FStateManager.ProcessStateVariables((rwby.StateTimeline)arg3, d, arg4, arg3.totalFrames, false);
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
                case VarRotateTowardsType.target:
                    
                    break;
                case VarRotateTowardsType.custom:
                    wantedDir = vars.eulerAngle;
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
            VarFindWall vars = (VarFindWall)variables;
            
            if(vars.clearWallIfNotFound) fm.ClearWall();
            
            Vector3 input = vars.inputSource == VarInputSourceType.stick ? fm.GetMovementVector(0) : fm.myTransform.forward;
            if(vars.normalizeInputSource) input.Normalize();
            if (input == Vector3.zero)
            {
                if (!vars.useRotationIfInputZero) return;
                    input = fm.transform.forward;
            }

            for (int i = 0; i < fm.wallHitResults.Length; i++)
            {
                fm.wallHitResults[i] = new RaycastHit();
            }
            
            var forward = fm.myTransform.forward;
            var right = fm.myTransform.right;
            var distance = fm.FPhysicsManager.ecbRadius + 0.2f;
            fm.Runner.GetPhysicsScene().Raycast(fm.GetCenter(), forward, out fm.wallHitResults[0],
                distance, fm.wallLayerMask, QueryTriggerInteraction.Ignore);
            fm.Runner.GetPhysicsScene().Raycast(fm.GetCenter(), (forward + right).normalized, out fm.wallHitResults[1],
                distance, fm.wallLayerMask, QueryTriggerInteraction.Ignore);
            fm.Runner.GetPhysicsScene().Raycast(fm.GetCenter(), right, out fm.wallHitResults[2],
                distance, fm.wallLayerMask, QueryTriggerInteraction.Ignore);
            fm.Runner.GetPhysicsScene().Raycast(fm.GetCenter(), (-forward + right), out fm.wallHitResults[3],
                distance, fm.wallLayerMask, QueryTriggerInteraction.Ignore);
            fm.Runner.GetPhysicsScene().Raycast(fm.GetCenter(), -forward, out fm.wallHitResults[4],
                distance, fm.wallLayerMask, QueryTriggerInteraction.Ignore);
            fm.Runner.GetPhysicsScene().Raycast(fm.GetCenter(), (-forward + -right).normalized, out fm.wallHitResults[5],
                distance, fm.wallLayerMask, QueryTriggerInteraction.Ignore);
            fm.Runner.GetPhysicsScene().Raycast(fm.GetCenter(), -right, out fm.wallHitResults[6],
                distance, fm.wallLayerMask, QueryTriggerInteraction.Ignore);
            fm.Runner.GetPhysicsScene().Raycast(fm.GetCenter(), (forward + -right), out fm.wallHitResults[7],
                distance, fm.wallLayerMask, QueryTriggerInteraction.Ignore);


            float min = Mathf.Infinity;
            int lowestIndex = -1;

            for (int i = 1; i < fm.wallHitResults.Length; i++)
            {
                if (fm.wallHitResults[i].point == Vector3.zero) continue;
                if (IsHitValid(fm, vars, fm.wallHitResults[i], input) && fm.wallHitResults[i].distance < min)
                {
                    lowestIndex = i;
                    min = fm.wallHitResults[i].distance;
                }
            }

            if (lowestIndex == -1) return;

            fm.AssignWall(fm.wallHitResults[lowestIndex]);
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
            VarSnapToWall vars = (VarSnapToWall)variables;

            if (!fm.IsWallValid()) return;

            Vector3 newPos = fm.cWallPoint + (fm.cWallNormal * fm.FPhysicsManager.ecbRadius) - (new Vector3(0, fm.FPhysicsManager.ecbOffset, 0));

            fm.FPhysicsManager.SetPosition(newPos, false);
            fm.SetRotation(-fm.cWallNormal, false);
        }
        
        public static void SnapToPole(IFighterBase fighter, IStateVariables variables, HnSF.StateTimeline arg3, int arg4)
        {
            FighterManager fm = (FighterManager)fighter;
            VarSnapToPole vars = (VarSnapToPole)variables;

            if (fm.foundPole == null) return;
            fm.FPhysicsManager.SetPosition(fm.foundPole.GetNearestPoint(fm.transform.position), false);
            fm.SetRotation(fm.foundPole.GetNearestFaceDirection(fm.transform.forward));
            fm.poleMagnitude = (fm.FPhysicsManager.forceMovement + new Vector3(0, fm.FPhysicsManager.forceGravity, 0)).magnitude;
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
            VarTeleportRaycast vars = (VarTeleportRaycast)variables;

            Vector3 dir = Vector3.zero;
            switch (vars.raycastDirectionSource)
            {
                case VarInputSourceType.stick:
                    break;
                case VarInputSourceType.rotation:
                    break;
                case VarInputSourceType.custom:
                    dir = fm.GetMovementVector(vars.direction.x, vars.direction.y);
                    dir.y = vars.direction.y;
                    break;
            }
            
            Vector3 bottomPoint = fm.myTransform.position + fm.FPhysicsManager.cc.center + Vector3.up * -fm.FPhysicsManager.cc.height * 0.5F;
            Vector3 topPoint = bottomPoint + Vector3.up * fm.FPhysicsManager.cc.height;

            bool hit = fm.Runner.GetPhysicsScene().CapsuleCast(bottomPoint, topPoint, fm.FPhysicsManager.cc.radius * 0.9f, dir, out fm.wallHitResults[0], vars.distance, fm.wallLayerMask.value);

            if (hit)
            {
                Vector3 newPos = fm.wallHitResults[0].point 
                                 + (new Vector3(fm.wallHitResults[0].normal.x, 0, fm.wallHitResults[0].normal.z) * fm.FPhysicsManager.cc.radius)
                                 - (new Vector3(0, fm.wallHitResults[0].normal.y, 0) * (bottomPoint - fm.myTransform.position).y );
                fm.FPhysicsManager.SetPosition(newPos, vars.bypassInterpolation);
            }
            else
            {
                if (!vars.goToPosOnNoHit) return;
                Vector3 newPos = fm.transform.position + (dir * vars.distance);
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
                    Debug.DrawRay(fm.myTransform.position, (fm.myTransform.forward * rotatedVector.z) + (fm.myTransform.right * rotatedVector.x)  + (Vector3.up * rotatedVector.y), Color.red, fm.Runner.DeltaTime);
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
    }
}