using System;
using UnityEngine;
using Fusion;
using HnSF.Combat;
using HnSF.Fighters;
using HnSF.Input;
using HnSF;

namespace rwby
{
    public class FighterCombatManager : NetworkBehaviour, IHurtable, IFighterCombatManager, ITeamable, IThrowable, IThrower
    {
        public static readonly int MAX_HARDKNOCKDOWNS = 2;
        public static readonly int MAX_BURST = 15000;
        public static readonly int AURALOCKOUTTIME = 300;
        [System.Serializable]
        public class IntIntMap
        {
            public int item1;
            public int item2;
        }

        [System.Serializable]
        public class IntFloatMap
        {
            public int item1;
            public float item2;
        }

        [Networked] public bool throwLocked { get; set; }
        [Networked] public NetworkObject thrower { get; set; }
        [Networked, Capacity(4)] public NetworkArray<NetworkObject> throwees => default;
        [Networked] public int ThrowTechTimer { get; set; } = 0;
        [Networked] public BlockStateType BlockState { get; set; }
        [Networked(OnChanged = nameof(OnChangedHitstun))] public int HitStun { get; set; } = -1;
        [Networked] public int HitStop { get; set; } = -1;
        [Networked] public int LastHitStop { get; set; } = -1;
        [Networked] public int BlockStun { get; set; } = -1;
        [Networked] public int BurstMeter { get; set; } = MAX_BURST;
        [Networked] public NetworkBool Charging { get; set; }
        [Networked] public int CurrentChargeLevel { get; set; }
        [Networked] public int CurrentChargeLevelCharge { get; set; }
        [Networked] public int ComboCounter { get; set; }
        [Networked] public int ComboStartTick { get; set; }
        [Networked] public float Proration { get; set; } = 1.0f;
        [Networked(OnChanged = nameof(OnChangedAura))] public int Aura { get; set; }
        public int AuraRegenDelay { get; set; } = 0;
        public FighterHitManager HitboxManager { get { return hitboxManager; } }

        [SerializeField] protected HealthManager healthManager;
        [SerializeField] protected FighterHitManager hitboxManager;
        [SerializeField] protected FighterManager manager;
        [SerializeField] protected FighterInputManager inputManager;
        [SerializeField] protected FighterPhysicsManager physicsManager;
        [SerializeField] protected FighterStateManager stateManager;

        [Networked] public int Team { get; set; }
        [Networked, Capacity(20)] public NetworkLinkedList<MovesetStateIdentifier> movesUsedInString => default;
        
        [Networked] public bool WallBounce { get; set; }
        [Networked] public float WallBounceForce { get; set; }
        [Networked] public bool GroundBounce { get; set; }
        [Networked] public float GroundBounceForce { get; set; }

        [Networked] public int CurrentWallBounces { get; set; } = 0;
        [Networked] public int CurrentGroundBounces { get; set; } = 0;

        [Networked, Capacity(4)] public NetworkArray<int> assignedSpecials => default;

        [Networked] public bool ClashState { get; set; }
        public bool CounterhitState;
        
        public IntIntMap[] hitstunDecayMap = new IntIntMap[4];
        public IntFloatMap[] pushbackScalingMap = new IntFloatMap[4];
        public IntFloatMap[] gravityScalingMap = new IntFloatMap[4];

        public delegate void ValueDelegate(FighterCombatManager combatManager, int maxValue);

        public event ValueDelegate OnAuraIncreased;
        public event ValueDelegate OnAuraDecreased;
        public event ValueDelegate OnHitstunIncreased;
        public event ValueDelegate OnHitstunDecreased;

        [Networked] public int LastSuccessfulBlockTick { get; set; }
        [Networked] public int LastSuccessfulPushblockTick { get; set; }
        [Networked] public bool CurrentlyPushblocking { get; set; }

        [Networked] public int lastUsedSpecial { get; set; }
        [Networked] public NetworkBool shouldHardKnockdown { get; set; }
        [Networked] public int hardKnockdownCounter { get; set; }

        public static void OnChangedAura(Changed<FighterCombatManager> changed)
        {
            changed.LoadOld();
            int oldAura = changed.Behaviour.Aura;
            changed.LoadNew();
            if (changed.Behaviour.Aura > oldAura)
            {
                changed.Behaviour.OnAuraIncreased?.Invoke(changed.Behaviour, changed.Behaviour.manager.fighterDefinition.Aura);
            }

            if (changed.Behaviour.Aura < oldAura)
            {
                changed.Behaviour.OnAuraDecreased?.Invoke(changed.Behaviour, changed.Behaviour.manager.fighterDefinition.Aura);
            }
        }

        public static void OnChangedHitstun(Changed<FighterCombatManager> changed)
        {
            changed.LoadOld();
            int oldHitstun = changed.Behaviour.HitStun;
            changed.LoadNew();
            if (changed.Behaviour.HitStun > oldHitstun)
            {
                changed.Behaviour.OnHitstunIncreased?.Invoke(changed.Behaviour, changed.Behaviour.HitStun);
            }else if (changed.Behaviour.HitStun < oldHitstun)
            {
                changed.Behaviour.OnHitstunDecreased?.Invoke(changed.Behaviour, changed.Behaviour.HitStun);
            }
        }

        public override void Spawned()
        {
            base.Spawned();
            for (int i = 0; i < 4; i++)
            {
                assignedSpecials.Set(i, i + 1);
            }
        }

        public virtual void Tick()
        {
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
            int movesetID = stateManager.CurrentMoveset;
            if (assignedSpecials[0] != 0 && inputManager.GetAbility1(out bOff).firstPress)
            {
                lastUsedSpecial = 0;
                int stateID = stateManager.CurrentGroundedState == StateGroundedGroupType.GROUND
                    ? stateManager.GetCurrentMoveset().specials[assignedSpecials[0] - 1].groundState.GetState()
                    : stateManager.GetCurrentMoveset().specials[assignedSpecials[0] - 1].aerialState.GetState();
                if (stateID == (int)FighterCmnStates.NULL) return false;
                if (!manager.FStateManager.CheckStateConditions(movesetID, stateID, stateManager.CurrentStateFrame,
                        false, true)) return false;
                stateManager.MarkForStateChange(stateID);
                return true;
            }
            if (assignedSpecials[1] != 0 && inputManager.GetAbility2(out bOff).firstPress)
            {
                lastUsedSpecial = 1;
                int s = stateManager.CurrentGroundedState == StateGroundedGroupType.GROUND
                    ? stateManager.GetCurrentMoveset().specials[assignedSpecials[1] - 1].groundState.GetState()
                    : stateManager.GetCurrentMoveset().specials[assignedSpecials[1] - 1].aerialState.GetState();
                if (s == (int)FighterCmnStates.NULL) return false;
                if (!manager.FStateManager.CheckStateConditions(movesetID, s, stateManager.CurrentStateFrame,
                        false, true)) return false;
                stateManager.MarkForStateChange(s);
                return true;
            }
            if (assignedSpecials[2] != 0 && inputManager.GetAbility3(out bOff).firstPress)
            {
                lastUsedSpecial = 2;
                int s = stateManager.CurrentGroundedState == StateGroundedGroupType.GROUND
                    ? stateManager.GetCurrentMoveset().specials[assignedSpecials[2] - 1].groundState.GetState()
                    : stateManager.GetCurrentMoveset().specials[assignedSpecials[2] - 1].aerialState.GetState();
                if (s == (int)FighterCmnStates.NULL) return false;
                if (!manager.FStateManager.CheckStateConditions(movesetID, s, stateManager.CurrentStateFrame,
                        false, true)) return false;
                stateManager.MarkForStateChange(s);
                return true;
            }
            if (assignedSpecials[3] != 0 && inputManager.GetAbility4(out bOff).firstPress)
            {
                lastUsedSpecial = 3;
                int s = stateManager.CurrentGroundedState == StateGroundedGroupType.GROUND
                    ? stateManager.GetCurrentMoveset().specials[assignedSpecials[3] - 1].groundState.GetState()
                    : stateManager.GetCurrentMoveset().specials[assignedSpecials[3] - 1].aerialState.GetState();
                if (s == (int)FighterCmnStates.NULL) return false;
                if (!manager.FStateManager.CheckStateConditions(movesetID, s, stateManager.CurrentStateFrame,
                        false, true)) return false;
                stateManager.MarkForStateChange(s);
                return true;
            }
            return false;
        }

        public virtual void ResetString()
        {
            movesUsedInString.Clear();
        }

        public virtual void ResetCharge()
        {
            SetChargeLevel(0);
            SetChargeLevelCharge(0);
        }

        //TODO: Figure out what to do when queue in filled.
        public virtual void AddMoveToString()
        {
            movesUsedInString.Add(new MovesetStateIdentifier(){ movesetIdentifier = stateManager.CurrentStateMoveset, stateIdentifier = stateManager.CurrentState });
        }

        public virtual void AddMoveToString(int currentStateMovement, int currentState)
        {
            movesUsedInString.Add(new MovesetStateIdentifier(){ movesetIdentifier = currentStateMovement, stateIdentifier = currentState});
        }

        public virtual bool MovePossible(MovesetStateIdentifier movesetState, int maxUsesInString = 1, int selfChainable = 0)
        {
            if (movesUsedInString.Count == 0) return true;
            if (selfChainable > 0)
            {
                int c = 0;
                for (int i = movesUsedInString.Count - 1; i >= 0; i--)
                {
                    if (movesUsedInString[i].movesetIdentifier == movesetState.movesetIdentifier
                        && movesUsedInString[i].stateIdentifier == movesetState.stateIdentifier)
                    {
                        c++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (c >= 1 && c < selfChainable) return true;
            }
                
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
            LastHitStop = value;
            HitStop = value;
            inputManager.ExtraBuffer = value;
        }

        public virtual void SetHitStun(int value)
        {
            HitStun = value;
        }

        public virtual void AddHitStop(int value)
        {
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
            if (Aura == 0) AuraRegenDelay = AURALOCKOUTTIME;
        }

        public virtual void SetAura(int value)
        {
            Aura = value;
            Aura = Mathf.Clamp(Aura, 0, manager.fighterDefinition.Aura);
            if (Aura == 0) AuraRegenDelay = AURALOCKOUTTIME;
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
            //ClearBuffer();

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
                        if (!inputManager.GetButton((int)PlayerInputType.LOCK_ON, out int loOffset, baseOffset, 0)
                                .isDown) return false;
                        if (CheckStickDirection(sequence.executeInputs[e].stickDirection, sequence.executeInputs[e].directionDeviation, baseOffset) == false)
                        {
                            return false;
                        }
                        break;
                    case HnSF.Input.InputDefinitionType.Button:
                        if (inputManager.GetButton(sequence.executeInputs[e].buttonID, out int gotOffset, baseOffset, (int)sequence.executeWindow+inputManager.ExtraBuffer).firstPress == false)
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
                        for (uint f = currentOffset; f < currentOffset + sequence.sequenceWindow + inputManager.ExtraBuffer; f++)
                        {
                            if (CheckStickDirection(sequence.sequenceInputs[s].stickDirection, sequence.sequenceInputs[s].directionDeviation, (int)f))
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
                        for (uint f = currentOffset; f < currentOffset + sequence.sequenceWindow + inputManager.ExtraBuffer; f++)
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
            if (CurrentChargeLevelCharge >= maxCharge)
            {
                SetChargeLevel(CurrentChargeLevel+1);
                CurrentChargeLevelCharge = 0;
            }
        }

        public LockonDirType stickDirectionCheck;
        public virtual bool CheckStickDirection(Vector2 stickDirection, float directionDeviation, int framesBack)
        {
            Vector2 stickDir = inputManager.GetMovement(framesBack);
            if (stickDirection == Vector2.zero)
            {
                if (stickDir.magnitude < InputConstants.movementDeadzone) return true;
                return false;
            }

            switch (stickDirectionCheck)
            {
                case LockonDirType.TargetRelative:
                    if (!manager.HardTargeting || manager.CurrentTarget == null) return CheckAttackerForwardDir(stickDir, stickDirection, directionDeviation);
                    Vector3 aForwardDir = manager.GetMovementVector(stickDir.x, stickDir.y);
                    Vector3 targetDirForward = manager.CurrentTarget.transform.position - manager.transform.position;
                    targetDirForward.y = 0;
                    Vector3 targetRight = Vector3.Cross(targetDirForward, Vector3.up);
                    Vector3 wantedDir = targetDirForward * stickDirection.y +
                                        targetRight * stickDirection.x;
                    
                    if (Vector3.Dot(aForwardDir, wantedDir) >= directionDeviation)
                    {
                        return true;
                    }
                    return false;
                case LockonDirType.AttackerForward:
                    return CheckAttackerForwardDir(stickDir, stickDirection, directionDeviation);
                case LockonDirType.Absolute:
                    if (Vector2.Dot(stickDir, stickDirection) >= directionDeviation)
                    {
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        protected virtual bool CheckAttackerForwardDir(Vector2 stickDir, Vector2 stickDirection, float directionDeviation)
        {
            Vector3 aForwardDir = manager.GetMovementVector(stickDir.x, stickDir.y).normalized;
            Vector3 aWantedDir = manager.GetVisualMovementVector(stickDirection.x, stickDirection.y).normalized;
            if (Vector3.Dot(aForwardDir, aWantedDir) >= directionDeviation)
            {
                return true;
            }
            return false;
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

        public float pushBlockForce = 5.0f;
        public float burstScaler1 = 50;
        public float burstScaler2 = 0.03f;
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

            if (isGrounded && hitInfoGroup.groundHitState == FighterCmnStates.NULL
                || !isGrounded && hitInfoGroup.airHitState == FighterCmnStates.NULL)
            {
                hitReaction.reaction = HitReactionType.HIT;
                return hitReaction;
            }
            
            SetHitStop(hitInfoGroup.hitstop);
            
            if(!hitInfoGroup.unblockable && BlockState != BlockStateType.NONE)
            {
                if((Runner.Tick - LastSuccessfulBlockTick) <= 7 || Vector3.Angle(transform.forward, hurtInfo.forward) > 90)
                {
                    hitReaction.reaction = HitReactionType.BLOCKED;
                    SetHitStop(hitInfoGroup.hitstop);
                    BlockStun = hitInfoGroup.blockstun;

                    Vector3 hForce = isGrounded ? hitInfoGroup.groundHitForce : hitInfoGroup.aerialHitForce;
                    if (!hitInfoGroup.blockLift) hForce.y = 0;

                    if (CurrentlyPushblocking)
                    {
                        LastSuccessfulPushblockTick = Runner.Tick + hitInfoGroup.hitstop;
                        
                        Vector3 temp = hurtInfo.center - manager.myTransform.position;
                        temp.y = 0;
                        temp.Normalize();
                        temp *= pushBlockForce;
                        hitReaction.pushback = temp;
                    }
                    
                    manager.HealthManager.ModifyHealth((int)-(hitInfoGroup.chipDamage));
                    ApplyHitForces(hurtInfo, currentState, hitInfoGroup.hitForceType, hForce, hitInfoGroup.pullPushMultiplier, 
                        hitInfoGroup.pullPushMaxDistance, hitInfoGroup.hitForceRelationOffset, true, liftOffGround: hitInfoGroup.blockLift);
                    manager.shakeDefinition = new CmaeraShakeDefinition()
                    {
                        shakeStrength = hitInfoGroup.blockCameraShakeStrength,
                        startFrame = Runner.Tick,
                        endFrame = Runner.Tick + hitInfoGroup.cameraShakeLength
                    };
                    LastSuccessfulBlockTick = Runner.Tick;
                    
                    return hitReaction;
                }
            }

            if (ComboCounter == 0) ComboStartTick = Runner.Tick;
            
            if(hurtInfo.hurtboxHit != -1) hurtboxHitCount.Set(hurtInfo.hurtboxHit, hurtboxHitCount[hurtInfo.hurtboxHit] + 1);
            manager.FPhysicsManager.SetRotation((hurtInfo.forward * -1).normalized);
            
            // Got hit, apply stun, damage, and forces.
            hitReaction.reaction = HitReactionType.HIT;
            int initHitstunValue = isGrounded ? hitInfoGroup.hitstun : hitInfoGroup.untech;
            SetHitStun(hitInfoGroup.ignoreHitstunScaling ? initHitstunValue : ApplyHitstunScaling(initHitstunValue));
            ApplyHitForces(hurtInfo, currentState, hitInfoGroup.hitForceType, isGrounded ?  hitInfoGroup.groundHitForce : hitInfoGroup.aerialHitForce, hitInfoGroup.pullPushMultiplier, hitInfoGroup.pullPushMaxDistance, hitInfoGroup.hitForceRelationOffset,
                hitInfoGroup.ignorePushbackScaling);

            shouldHardKnockdown = hardKnockdownCounter > MAX_HARDKNOCKDOWNS ? false : hitInfoGroup.hardKnockdown;
            WallBounce = hitInfoGroup.wallBounce;
            WallBounceForce = hitInfoGroup.wallBounceForce;
            GroundBounce = hitInfoGroup.groundBounce;
            GroundBounceForce = hitInfoGroup.groundBounceForce;

            if (hitInfoGroup.autolink)
            {
                Vector3 calcForce = hurtInfo.attackerVelocity;
                physicsManager.forceGravity += calcForce.y * hitInfoGroup.gravityAutolinkPercentage;
                calcForce.y = 0;
                physicsManager.forceMovement += calcForce * hitInfoGroup.movementAutolinkPercentage;
            }
            
            if (physicsManager.forceGravity > 0)
            {
                physicsManager.SetGrounded(false);
            }

            int dmg = (int)(hitInfoGroup.damage * (hitInfoGroup.ignoreProration ? 1.0f : Proration));
            if (hitInfoGroup.damage > 0) dmg = Mathf.Clamp(dmg, 1, Int32.MaxValue);
            manager.Hurt(hurtInfo.team, -dmg);

            stateManager.ChangeState(isGrounded ? (int)hitInfoGroup.groundHitState : (int)hitInfoGroup.airHitState);
            manager.FCombatManager.Cleanup();
            ComboCounter++;

            if (ComboCounter == 1)
            {
                Proration = (1.0f - hitInfoGroup.initialProration) * 0.60f;
            }
            else
            {
                Proration *= (1.0f - hitInfoGroup.comboProration);
            }
            
            BurstMeter += (int)((burstScaler1 + dmg * 3) * (1.0f + ComboCounter * burstScaler2));
            BurstMeter = Mathf.Clamp(BurstMeter, 0, MAX_BURST);
            
            manager.shakeDefinition = new CmaeraShakeDefinition()
            {
                shakeStrength = hitInfoGroup.hitCameraShakeStrength,
                startFrame = Runner.Tick,
                endFrame = Runner.Tick + hitInfoGroup.cameraShakeLength
            };
            return hitReaction;
        }

        
        public virtual int ApplyHitstunScaling(int hitstunAmt)
        {
            int comboTime = Runner.Tick - ComboStartTick;
            if (comboTime < hitstunDecayMap[0].item1) return hitstunAmt;
            for (int i = hitstunDecayMap.Length - 1; i >= 0; i--)
            {
                if (comboTime > hitstunDecayMap[i].item1)
                {
                    if (i == hitstunDecayMap.Length - 1) return 1;
                    return Mathf.Clamp(hitstunAmt - hitstunDecayMap[i].item2, 0, Int32.MaxValue);
                }
            }
            return hitstunAmt;
        }

        public virtual Vector3 ApplyPushbackScaling(Vector3 pushback)
        {
            int comboTime = Runner.Tick - ComboStartTick;
            if (comboTime < pushbackScalingMap[0].item1) return pushback;
            for (int i = pushbackScalingMap.Length - 1; i >= 0; i--)
            {
                if (comboTime > pushbackScalingMap[i].item1)
                {
                    return pushback * pushbackScalingMap[i].item2;
                }
            }
            return pushback;
        }
        
        public virtual float ApplyGravityScaling(float gravity)
        {
            int comboTime = Runner.Tick - ComboStartTick;
            if (comboTime < gravityScalingMap[0].item1) return gravity;
            for (int i = gravityScalingMap.Length - 1; i >= 0; i--)
            {
                if (comboTime > gravityScalingMap[i].item1)
                {
                    return gravity * gravityScalingMap[i].item2;
                }
            }
            return gravity;
        }

        protected void ApplyHitForces(HurtInfo hurtInfo, StateTimeline currentState, HitboxForceType forceType, Vector3 force = default, float pullPushMulti = default,
            float pullPushMaxDistance = default, Vector3 offset = default, bool ignorePushbackScaling = false, bool ignoreGravityScaling = false, 
            bool liftOffGround = true)
        {
            
            switch (forceType)
            {
                case HitboxForceType.SET:
                    Vector3 forces = (force.x * hurtInfo.right) + (force.z * hurtInfo.forward);
                    if(liftOffGround) physicsManager.forceGravity = ignoreGravityScaling ? force.y : ApplyGravityScaling(force.y);
                    physicsManager.forceMovement = ignorePushbackScaling ? forces : ApplyPushbackScaling(forces);
                    break;
                case HitboxForceType.PULL:
                    var position = manager.GetCenter();
                    var centerPos = hurtInfo.center + (hurtInfo.right * offset.x) + (hurtInfo.forward * offset.z) + (Vector3.up * offset.y);

                    float gIntensity = Vector3.Distance(position, centerPos) / pullPushMaxDistance;
                    var pF = (centerPos - position) * gIntensity * pullPushMulti;
                    if(liftOffGround) physicsManager.forceGravity = pF.y;
                    pF.y = 0;
                    physicsManager.forceMovement = pF;
                    break;
                case HitboxForceType.PUSH:
                    var pPush = manager.GetCenter();
                    var centerPosPush = hurtInfo.center + (hurtInfo.right * offset.x) + (hurtInfo.forward * offset.z) + (Vector3.up * offset.y);
                    
                    var forcePush = (pPush - Vector3.MoveTowards(pPush, centerPosPush, pullPushMaxDistance)) * pullPushMulti;

                    if(liftOffGround) physicsManager.forceGravity = forcePush.y;
                    forcePush.y = 0;
                    physicsManager.forceMovement = forcePush;
                    break;
            }
        }
        
        #region Throwable
        public void ThroweeInitilization(NetworkObject throwerObject)
        {
            throwLocked = true;
            thrower = throwerObject;
            manager.FStateManager.MarkForStateChange((int)FighterCmnStates.THROWN, force: true);
        }

        public void TechThrow()
        {
            throwLocked = false;
            ThrowTechTimer = 0;
            manager.FStateManager.ForceStateChange((int)FighterCmnStates.THROW_TECH, clearMarkedState: true);
            thrower.GetComponent<IThrower>().OnThrowTeched(Object);
        }

        public void SetThroweePosition(Vector3 position)
        {
            
        }

        public void SetThroweeRotation(Vector3 rotation)
        {
            
        }
        #endregion
        
        #region Thrower

        public bool IsThroweeValid(CustomHitbox attackerThrowbox, Throwablebox attackeeThrowablebox)
        {
            if (attackeeThrowablebox.ownerNetworkObject == Object || throwLocked == true) return false;
            var attackeeF = attackeeThrowablebox.ownerNetworkObject.gameObject.GetComponent<FighterManager>();
            if (attackeeF.FPhysicsManager.IsGrounded &&
                attackerThrowbox.definition.ThrowboxInfo[attackerThrowbox.definitionIndex].airOnly
                || !attackeeF.FPhysicsManager.IsGrounded && attackerThrowbox.definition
                    .ThrowboxInfo[attackerThrowbox.definitionIndex].groundOnly) return false;
            return true;
        }

        public void ThrowerInitilization(NetworkObject throwee)
        {
            throwees.Set(0, throwee);
        }

        public void OnThrowTeched(NetworkObject throwee)
        {
            for (int i = 0; i < throwees.Length; i++)
            {
                throwees.Set(i, null);
            }
            manager.FStateManager.ForceStateChange((int)FighterCmnStates.THROW_TECH, clearMarkedState: true);
        }

        public void ReleaseThrowee(NetworkObject throwee)
        {
            if (throwee)
            {
                var t = throwee.GetComponent<IThrowable>();
                t.throwLocked = false;
            }

            for (int i = 0; i < throwees.Length; i++)
            {
                throwees.Set(i, null);
            }
        }
        #endregion
    }
}