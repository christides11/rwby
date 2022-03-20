using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using HnSF.Combat;
using HnSF.Fighters;
using HnSF.Input;
using System;
using HnSF;

namespace rwby
{
    public class FighterCombatManager : NetworkBehaviour, IHurtable, IFighterCombatManager, ITeamable
    {
        [Networked] public BlockStateType BlockState { get; set; }
        [Networked] public int HitStun { get; set; }
        [Networked] public int HitStop { get; set; }
        [Networked] public int BlockStun { get; set; }
        [Networked] public NetworkBool Charging { get; set; }
        [Networked] public int CurrentChargeLevel { get; set; }
        [Networked] public int CurrentChargeLevelCharge { get; set; }
        public FighterHitManager HitboxManager { get { return hitboxManager; } }

        [SerializeField] protected HealthManager healthManager;
        [SerializeField] protected FighterHitManager hitboxManager;
        [SerializeField] protected FighterManager manager;
        [SerializeField] protected FighterInputManager inputManager;
        [SerializeField] protected FighterPhysicsManager physicsManager;
        [SerializeField] protected FighterStateManager stateManager;

        [Networked] public int Team { get; set; }
        [Networked, Capacity(10)] public NetworkLinkedList<MovesetStateIdentifier> movesUsedInString => default;

        [Networked] public int hitstopCounter { get; set; }
        [Networked] public NetworkBool CounterhitState { get; set; }
        [Networked] public int WallBounces { get; set; }
        [Networked] public float WallBounceForcePercentage { get; set; }
        [Networked] public int GroundBounces { get; set; }
        [Networked] public float GroundBounceForcePercentage { get; set; }

        public virtual void ResetString()
        {
            movesUsedInString.Clear();
        }

        public virtual bool MovePossible(MovesetStateIdentifier movesetState, int maxUsesInString = 1)
        {
            int counter = 0;
            for (int i = 0; i < movesUsedInString.Count; i++)
            {
                if (movesUsedInString[i].movesetIdentifier == movesetState.movesetIdentifier
                    && movesUsedInString[i].stateIdentifier == movesetState.stateIdentifier) counter++;
                if (counter == maxUsesInString) return false;
            }

            if (counter >= maxUsesInString) return false;
            return true;
        }
        
        public virtual void SetHitStop(int value)
        {
            hitstopCounter = 0;
            HitStop = value;
        }

        public virtual void SetHitStun(int value)
        {
            HitStun = value;
        }

        public virtual void AddHitStop(int value)
        {
            hitstopCounter = 0;
            HitStop += value;
        }

        public virtual void AddHitStun(int value)
        {
            HitStun += value;
        }

        public void Cleanup()
        {
            CurrentChargeLevel = 0;
            CurrentChargeLevelCharge = 0;
            hitboxManager.Reset();
        }

        /// <summary>
        /// Checks to see if a given input sequence was inputted.
        /// </summary>
        /// <param name="sequence">The sequence we're looking for.</param>
        /// <param name="baseOffset">How far back we want to start the sequence check. 0 = current frame, 1 = 1 frame back, etc.</param>
        /// <param name="processSequenceButtons">If the sequence buttons should be checked, even if the execute buttons were not pressed or don't exist.</param>
        /// <param name="holdInput">If the sequence check should check for the buttons being held down instead of their first prcess.</param>
        /// <returns>True if the input sequence was inputted.</returns>
        public virtual bool CheckForInputSequence(InputSequence sequence, uint baseOffset = 0, bool processSequenceButtons = false, bool holdInput = false)
        {
            uint currentOffset = 0;
            bool executeInputsSuccessful = CheckExecuteInputs(sequence, (int)baseOffset, ref currentOffset);

            if (sequence.executeInputs.Count == 0)
            {
                currentOffset++;
                if (processSequenceButtons == false)
                {
                    executeInputsSuccessful = false;
                }
            }

            // We did not press the buttons required for this move.
            if (executeInputsSuccessful == false)
            {
                return false;
            }
            ClearBuffer();

            bool sequenceInputsSuccessful = CheckSequenceInputs(sequence, holdInput, ref currentOffset);

            if (sequenceInputsSuccessful == false)
            {
                return false;
            }
            return true;
        }

                protected virtual bool CheckExecuteInputs(InputSequence sequence, int baseOffset, ref uint currentOffset)
        {
            for (int e = 0; e < sequence.executeInputs.Count; e++)
            {
                switch (sequence.executeInputs[e].inputType)
                {
                    case HnSF.Input.InputDefinitionType.Stick:
                        if (CheckStickDirection(sequence.executeInputs[e], baseOffset) == false)
                        {
                            return false;
                        }
                        break;
                    case HnSF.Input.InputDefinitionType.Button:
                        if (inputManager.GetButton((PlayerInputType)sequence.executeInputs[e].buttonID, out int gotOffset, baseOffset, (int)sequence.executeWindow).firstPress == false)
                        {
                            return false;
                        }
                        if (gotOffset >= currentOffset)
                        {
                            currentOffset = (uint)gotOffset;
                        }
                        break;
                }
            }
            return true;
        }

        protected virtual bool CheckSequenceInputs(InputSequence sequence, bool holdInput, ref uint currentOffset)
        {
            for (int s = 0; s < sequence.sequenceInputs.Count; s++)
            {
                bool foundInput = false;
                switch (sequence.sequenceInputs[s].inputType)
                {
                    case HnSF.Input.InputDefinitionType.Stick:
                        for (uint f = currentOffset; f < currentOffset + sequence.sequenceWindow; f++)
                        {
                            if (CheckStickDirection(sequence.sequenceInputs[s], (int)f))
                            {
                                foundInput = true;
                                currentOffset = f;
                                break;
                            }
                        }
                        if (foundInput == false)
                        {
                            return false;
                        }
                        break;
                    case HnSF.Input.InputDefinitionType.Button:
                        for (uint f = currentOffset; f < currentOffset + sequence.sequenceWindow; f++)
                        {
                            if ((!holdInput && inputManager.GetButton((PlayerInputType)sequence.sequenceInputs[s].buttonID, out int gotOffset, (int)f, 0).firstPress)
                                || (holdInput && inputManager.GetButton((PlayerInputType)sequence.sequenceInputs[s].buttonID, out int gotOffsetTwo, (int)f, 0).isDown))
                            {
                                foundInput = true;
                                currentOffset = f;
                                break;
                            }
                        }
                        if (foundInput == false)
                        {
                            return false;
                        }
                        break;
                }
            }
            return true;
        }

        public int GetTeam()
        {
            return Team;
        }

        public virtual void SetChargeLevel(int value)
        {
            CurrentChargeLevel = value;
        }

        public virtual void SetChargeLevelCharge(int value)
        {
            CurrentChargeLevelCharge = value;
        }

        public virtual void IncrementChargeLevelCharge(int maxCharge)
        {
            CurrentChargeLevelCharge++;
        }

        public LockonDirType stickDirectionCheck;
        protected virtual bool CheckStickDirection(HnSF.Input.InputDefinition sequenceInput, int framesBack)
        {
            Vector2 stickDir = inputManager.GetMovement(framesBack);
            if (stickDir.magnitude < InputConstants.movementDeadzone)
            {
                return false;
            }

            switch (stickDirectionCheck)
            {
                case LockonDirType.TargetRelative:
                    return false;
                case LockonDirType.AttackerForward:
                    Vector3 aForwardDir = manager.GetVisualMovementVector(stickDir.x, stickDir.y);
                    Vector3 aWantedDir = manager.GetVisualMovementVector(sequenceInput.stickDirection.x, sequenceInput.stickDirection.y);
                    if (Vector3.Dot(aForwardDir, aWantedDir) >= sequenceInput.directionDeviation)
                    {
                        return true;
                    }
                    return false;
                case LockonDirType.Absolute:
                    if (Vector2.Dot(stickDir, sequenceInput.stickDirection) >= sequenceInput.directionDeviation)
                    {
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        protected virtual void ClearBuffer()
        {
            inputManager.BufferLimit = Runner.Simulation.Tick;
        }

        public void Heal(HealInfoBase healInfo)
        {
            throw new System.NotImplementedException();
        }
        
        [Networked, Capacity(10)] public NetworkArray<int> hurtboxHitCount { get; }

        public HitReactionBase Hurt(HurtInfoBase hurtInfoBase)
        {
            HurtInfo hurtInfo = hurtInfoBase as HurtInfo;
            HitInfo hitInfo = hurtInfo.hitInfo as HitInfo;
            HitReaction hitReaction = new HitReaction();
            hitReaction.reaction = HitReactionType.AVOIDED;
            var currentState = stateManager.GetState();

            if (!hitInfo.hitStateGroups.HasFlag(currentState.stateGroup)) return hitReaction;

            bool groundedState = currentState.stateGroup == StateGroupType.GROUND;
            if(BlockState != BlockStateType.NONE)
            {
                if(Vector3.Angle(transform.forward, hurtInfo.forward) > 90)
                {
                    hitReaction.reaction = HitReactionType.BLOCKED;
                    SetHitStop(hitInfo.hitstop);
                    BlockStun = groundedState ? hitInfo.groundBlockstun : hitInfo.aerialBlockstun;
                    
                    ApplyHitForces(hitInfo.hitForceType, groundedState ? hitInfo.groundBlockForce : hitInfo.aerialBlockForce, hurtInfo);
                    return hitReaction;
                }
            }
            
            //int indexOfHurtboxGroup = currentState.HurtboxInfo[hurtInfo.hurtboxHit];
            //hurtboxHitCount.Set(hurtInfo.hurtboxHit, hurtboxHitCount[hurtInfo.hurtboxHit] + 1);
            
            manager.FPhysicsManager.SetRotation((hurtInfo.forward * -1).normalized);
            // Got hit, apply stun, damage, and forces.
            hitReaction.reaction = HitReactionType.HIT;
            SetHitStop(hitInfo.hitstop + (CounterhitState ? hitInfo.counterHitAddedHitstop : 0));
            SetHitStun(CounterhitState ? (groundedState ? hitInfo.groundCounterHitstun : hitInfo.aerialCounterHitstun) : (groundedState ? hitInfo.groundHitstun : hitInfo.aerialHitstun));
            Vector3 baseForce = CounterhitState ? (groundedState ? hitInfo.groundCounterHitForce : hitInfo.aerialCounterHitForce) : (groundedState ? hitInfo.groundHitForce : hitInfo.aerialHitForce);
            ApplyHitForces(hitInfo.hitForceType, baseForce, hurtInfo);

            WallBounces = hitInfo.wallBounces;
            WallBounceForcePercentage = hitInfo.wallBounceForcePercentage;
            GroundBounces = hitInfo.groundBounces;
            GroundBounceForcePercentage = hitInfo.groundBounceForcePercentage;

            if (hitInfo.autolink)
            {
                Vector3 calcForce = hurtInfo.attackerVelocity * hitInfo.autolinkPercentage;
                physicsManager.forceGravity += calcForce.y;
                calcForce.y = 0;
                physicsManager.forceMovement += calcForce;
            }
            
            if (physicsManager.forceGravity > 0)
            {
                physicsManager.SetGrounded(false);
            }

            switch (currentState.stateGroup)
            {
                case StateGroupType.GROUND:
                    stateManager.ChangeState(CounterhitState ? (int)hitInfo.groundCounterHitState : (int)hitInfo.groundHitState);
                    break;
                case StateGroupType.AERIAL:
                    stateManager.ChangeState(CounterhitState ? (int)hitInfo.aerialCounterHitState : (int)hitInfo.aerialHitState);
                    break;
            }
            manager.FCombatManager.Cleanup();
            return hitReaction;
        }

        protected void ApplyHitForces(HitboxForceType forceType, Vector3 baseForce, HurtInfo hurtInfo)
        {
            switch (forceType)
            {
                case HitboxForceType.SET:
                    Vector3 forces = (baseForce.x * hurtInfo.right) + (baseForce.z * hurtInfo.forward);
                    physicsManager.forceGravity = baseForce.y;
                    physicsManager.forceMovement = forces;
                    break;
                case HitboxForceType.PULL:
                    /*Vector3 pullDir = Vector3.ClampMagnitude((hurtInfo.center - (transform.position + Vector3.up)) * hitInfo.opponentForceMultiplier, hitInfo.opponentMaxMagnitude);
                    if (pullDir.magnitude < hitInfo.opponentMinMagnitude)
                    {
                        pullDir = (hurtInfo.center - (transform.position + Vector3.up) ).normalized * hitInfo.opponentMinMagnitude;
                    }
                    if (hitInfo.forceIncludeYForce)
                    {
                        physicsManager.forceGravity = pullDir.y;
                    }
                    pullDir.y = 0;
                    physicsManager.forceMovement = pullDir;*/
                    break;
                case HitboxForceType.PUSH:
                    break;
            }
        }
    }
}