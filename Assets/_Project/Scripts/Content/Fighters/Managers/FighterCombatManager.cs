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
        [Networked] public int ComboCounter { get; set; }
        [Networked] public int ComboTime { get; set; }
        [Networked] public float Proration { get; set; } = 1.0f;
        [Networked(OnChanged = nameof(OnChangedAura))] public int Aura { get; set; }
        public int AuraRegenDelay { get; set; }
        public FighterHitManager HitboxManager { get { return hitboxManager; } }

        [SerializeField] protected HealthManager healthManager;
        [SerializeField] protected FighterHitManager hitboxManager;
        [SerializeField] protected FighterManager manager;
        [SerializeField] protected FighterInputManager inputManager;
        [SerializeField] protected FighterPhysicsManager physicsManager;
        [SerializeField] protected FighterStateManager stateManager;

        [Networked] public int Team { get; set; }
        [Networked, Capacity(20)] public NetworkLinkedList<MovesetStateIdentifier> movesUsedInString => default;

        [Networked] public int hitstopCounter { get; set; }
        [Networked] public NetworkBool CounterhitState { get; set; }
        [Networked] public int WallBounces { get; set; }
        [Networked] public float WallBounceForcePercentage { get; set; }
        [Networked] public int GroundBounces { get; set; }
        [Networked] public float GroundBounceForcePercentage { get; set; }

        [Networked, Capacity(4)] public NetworkArray<int> assignedSpecials => default;

        public delegate void BehaviourDelegate(FighterCombatManager combatManager);

        public event BehaviourDelegate OnAuraIncreased;
        public event BehaviourDelegate OnAuraDecreased;
        
        public static void OnChangedAura(Changed<FighterCombatManager> changed)
        {
            changed.LoadOld();
            int oldHealth = changed.Behaviour.Aura;
            changed.LoadNew();
            if (changed.Behaviour.Aura > oldHealth)
            {
                changed.Behaviour.OnAuraIncreased?.Invoke(changed.Behaviour);
            }

            if (changed.Behaviour.Aura < oldHealth)
            {
                changed.Behaviour.OnAuraDecreased?.Invoke(changed.Behaviour);
            }
        }
        
        public override void Spawned()
        {
            base.Spawned();
            for (int i = 0; i < 2; i++)
            {
                assignedSpecials.Set(i, i + 1);
            }
        }

        public virtual void Tick()
        {
            if (stateManager.CurrentStateType == StateType.RECOVERY)
            {
                ComboTime++;
            }

            if (AuraRegenDelay == 0)
            {
                AddAura(manager.fighterDefinition.AuraGainPerFrame);
            }
            else
            {
                AuraRegenDelay--;
            }
        }

        public virtual bool TrySpecial()
        {
            int bOff = 0;
            var currentState = stateManager.GetState();
            if (assignedSpecials[0] != 0 && inputManager.GetAbility1(out bOff).firstPress)
            {
                stateManager.MarkForStateChange(stateManager.CurrentGroundedState == StateGroundedGroupType.GROUND
                    ? stateManager.GetMoveset().specials[assignedSpecials[0] - 1].groundState.GetState()
                    : stateManager.GetMoveset().specials[assignedSpecials[0] - 1].aerialState.GetState());
                return true;
            }
            if (assignedSpecials[1] != 0 && inputManager.GetAbility2(out bOff).firstPress)
            {
                stateManager.MarkForStateChange(stateManager.CurrentGroundedState == StateGroundedGroupType.GROUND
                    ? stateManager.GetMoveset().specials[assignedSpecials[1] - 1].groundState.GetState()
                    : stateManager.GetMoveset().specials[assignedSpecials[1] - 1].aerialState.GetState());
                return true;
            }
            if (assignedSpecials[2] != 0 && inputManager.GetAbility3(out bOff).firstPress)
            {
                stateManager.MarkForStateChange(stateManager.CurrentGroundedState == StateGroundedGroupType.GROUND
                    ? stateManager.GetMoveset().specials[assignedSpecials[2] - 1].groundState.GetState()
                    : stateManager.GetMoveset().specials[assignedSpecials[2] - 1].aerialState.GetState());
                return true;
            }
            if (assignedSpecials[3] != 0 && inputManager.GetAbility4(out bOff).firstPress)
            {
                stateManager.MarkForStateChange(stateManager.CurrentGroundedState == StateGroundedGroupType.GROUND
                    ? stateManager.GetMoveset().specials[assignedSpecials[3] - 1].groundState.GetState()
                    : stateManager.GetMoveset().specials[assignedSpecials[3] - 1].aerialState.GetState());
                return true;
            }
            return false;
        }

        public virtual void ResetString()
        {
            movesUsedInString.Clear();
        }

        public virtual void AddMoveToString()
        {
            movesUsedInString.Add(new MovesetStateIdentifier(){ movesetIdentifier = stateManager.CurrentStateMoveset, stateIdentifier = stateManager.CurrentState });
        }

        public virtual void AddMoveToString(int currentStateMovement, int currentState)
        {
            movesUsedInString.Add(new MovesetStateIdentifier(){ movesetIdentifier = currentStateMovement, stateIdentifier = currentState});
        }

        public virtual bool MovePossible(MovesetStateIdentifier movesetState, int maxUsesInString = 1)
        {
            if (maxUsesInString == -1) return true;
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

        public virtual void ResetProration()
        {
            Proration = 1.0f;
        }

        public virtual void AddAura(int value)
        {
            Aura += value;
            Aura = Mathf.Clamp(Aura, 0, manager.fighterDefinition.Aura);
        }

        public virtual void SetAura(int value)
        {
            Aura = value;
            Aura = Mathf.Clamp(Aura, 0, manager.fighterDefinition.Aura);
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
                        if (inputManager.GetButton(sequence.executeInputs[e].buttonID, out int gotOffset, baseOffset, (int)sequence.executeWindow).firstPress == false)
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
                            if ((!holdInput && inputManager.GetButton(sequence.sequenceInputs[s].buttonID, out int gotOffset, (int)f, 0).firstPress)
                                || (holdInput && inputManager.GetButton(sequence.sequenceInputs[s].buttonID, out int gotOffsetTwo, (int)f, 0).isDown))
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
            inputManager.BufferLimit = (uint)Runner.Simulation.Tick.Raw;
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
            
            bool isGrounded = stateManager.CurrentGroundedState == StateGroundedGroupType.GROUND;
            HitInfo.HitInfoGroup hitInfoGroup = CounterhitState ? hitInfo.counterhit : hitInfo.hit;
            hitReaction.hitInfoGroup = hitInfoGroup;

            if (hitInfoGroup.groundHitState == FighterCmnStates.NULL)
            {
                hitReaction.reaction = HitReactionType.HIT;
                return hitReaction;
            }
            
            if(BlockState != BlockStateType.NONE)
            {
                if(Vector3.Angle(transform.forward, hurtInfo.forward) > 90)
                {
                    hitReaction.reaction = HitReactionType.BLOCKED;
                    SetHitStop(hitInfoGroup.hitstop);
                    BlockStun = hitInfoGroup.blockstun;
                    
                    ApplyHitForces(hurtInfo, currentState, hitInfoGroup.hitForceType, isGrounded ? hitInfoGroup.groundHitForce : hitInfoGroup.aerialHitForce, hitInfoGroup.pullPushCurve, hitInfoGroup.pullPushMaxDistance, hitInfoGroup.hitForceRelationOffset);
                    return hitReaction;
                }
            }
            
            hurtboxHitCount.Set(hurtInfo.hurtboxHit, hurtboxHitCount[hurtInfo.hurtboxHit] + 1);
            manager.FPhysicsManager.SetRotation((hurtInfo.forward * -1).normalized);
            
            // Got hit, apply stun, damage, and forces.
            hitReaction.reaction = HitReactionType.HIT;
            SetHitStop(hitInfoGroup.hitstop);
            SetHitStun(isGrounded ? hitInfoGroup.hitstun : hitInfoGroup.untech);
            ApplyHitForces(hurtInfo, currentState, hitInfoGroup.hitForceType, isGrounded ?  hitInfoGroup.groundHitForce : hitInfoGroup.aerialHitForce, hitInfoGroup.pullPushCurve, hitInfoGroup.pullPushMaxDistance, hitInfoGroup.hitForceRelationOffset);
            
            WallBounces = hitInfoGroup.wallBounces;
            WallBounceForcePercentage = hitInfoGroup.wallBounceForcePercentage;
            GroundBounces = hitInfoGroup.groundBounces;
            GroundBounceForcePercentage = hitInfoGroup.groundBounceForcePercentage;

            if (hitInfoGroup.autolink)
            {
                Vector3 calcForce = hurtInfo.attackerVelocity * hitInfoGroup.autolinkPercentage;
                physicsManager.forceGravity += calcForce.y;
                calcForce.y = 0;
                physicsManager.forceMovement += calcForce;
            }
            
            if (physicsManager.forceGravity > 0)
            {
                physicsManager.SetGrounded(false);
            }

            stateManager.ChangeState(isGrounded ? (int)hitInfoGroup.groundHitState : (int)hitInfoGroup.airHitState);
            manager.FCombatManager.Cleanup();
            ComboCounter++;

            if (ComboCounter == 1)
            {
                ResetProration();
                Proration = hitInfoGroup.initialProration;
            }
            else
            {
                if (Proration > hitInfoGroup.forcedProration) Proration = hitInfoGroup.forcedProration;
            }
            return hitReaction;
        }

        protected void ApplyHitForces(HurtInfo hurtInfo, StateTimeline currentState, HitboxForceType forceType, Vector3 force = default, AnimationCurve pullPushCurve = default,
            float pullPushMaxDistance = default, Vector3 offset = default)
        {
            
            switch (forceType)
            {
                case HitboxForceType.SET:
                    Vector3 forces = (force.x * hurtInfo.right) + (force.z * hurtInfo.forward);
                    physicsManager.forceGravity = force.y;
                    physicsManager.forceMovement = forces;
                    break;
                case HitboxForceType.PULL:
                    var position = manager.myTransform.position;
                    var centerPos = hurtInfo.center + (hurtInfo.right * offset.x) + (hurtInfo.forward * offset.z) + (Vector3.up * offset.y);
                    
                    Vector3 dir = (centerPos - (position + Vector3.up)).normalized;
                    float t = Mathf.Clamp(Vector3.Distance(centerPos, (position + Vector3.up)), 0.0f, 1.0f);
                    dir *= pullPushCurve.Evaluate(t);
                    
                    physicsManager.forceGravity = dir.y;
                    dir.y = 0;

                    physicsManager.forceMovement = dir;
                    break;
                case HitboxForceType.PUSH:
                    break;
            }
        }
    }
}