using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using HnSF.Combat;
using HnSF.Fighters;
using HnSF.Input;
using System;
using rwby.Combat.AttackEvents;

namespace rwby
{
    public class FighterCombatManager : NetworkBehaviour, IHurtable, IFighterCombatManager
    {
        [Networked] public BlockStateType BlockState { get; set; }
        [Networked] public int HitStun { get; set; }
        [Networked] public int HitStop { get; set; }
        [Networked] public ushort BlockStun { get; set; }
        [Networked] public NetworkBool Charging { get; set; }
        [Networked] public int CurrentChargeLevel { get; set; }
        [Networked] public int CurrentChargeLevelCharge { get; set; }
        [Networked] public int CurrentMovesetIdentifier { get; set; }
        /// <summary>
        /// The identifier of the moveset that the current attack belongs to. Not the same as our current moveset.
        /// </summary>
        [Networked] public int CurrentAttackMovesetIdentifier { get; set; }
        [Networked] public int CurrentAttackNodeIdentifier { get; set; }
        public MovesetDefinition CurrentMoveset { get { return GetMoveset(CurrentMovesetIdentifier); } }
        /// <summary>
        /// The moveset that the current attack belongs to. Not the same as our current moveset.
        /// </summary>
        public MovesetDefinition CurrentAttackMoveset { get { return GetMoveset(CurrentAttackMovesetIdentifier); } }
        public HnSF.Combat.MovesetAttackNode CurrentAttackNode
        {
            get
            {
                if (CurrentAttackNodeIdentifier < 0) { return null; }
                return (MovesetAttackNode)GetMoveset(CurrentAttackMovesetIdentifier).GetAttackNode(CurrentAttackNodeIdentifier);
            }
        }

        public FighterHitManager HitboxManager { get { return hitboxManager; } }

        [SerializeField] protected HealthManager healthManager;
        [SerializeField] protected FighterHitManager hitboxManager;
        [SerializeField] protected FighterManager manager;
        [SerializeField] protected FighterInputManager inputManager;
        [SerializeField] protected FighterPhysicsManager physicsManager;
        [SerializeField] protected FighterStateManager stateManager;

        [Networked] public int Team { get; set; }

        public MovesetDefinition[] movesets;

        [Networked] public int hitstunGravityHangTime { get; set; }
        [Networked] public float hitstunGravityHangTimer { get; set; }
        [Networked] public float hitstunHoldVelocityTime { get; set; }
        [Networked] public float hitstunFriction { get; set; }
        [Networked] public float hitstunGravity { get; set; }

        [Networked] public int hitstopCounter { get; set; }

        [Networked, Capacity(20)] public NetworkLinkedList<MovesetAttackStringIdentifier> attacksUsedInString { get; }

        public bool StringAttackValid(byte moveset, byte attack, byte maxTimes)
        {
            bool found = false;
            for(int i = 0; i < attacksUsedInString.Count; i++)
            {
                if(attacksUsedInString[i].moveset == moveset
                    && attacksUsedInString[i].attack == attack)
                {
                    found = true;
                    if (attacksUsedInString[i].timesUsed < maxTimes) return true;
                }
            }
            if (!found) return true;
            return false;
        }

        public void ReportStringAttack()
        {
            ReportStringAttack((byte)CurrentAttackMovesetIdentifier, (byte)CurrentAttackNodeIdentifier);
        }

        public void ReportStringAttack(byte moveset, byte attack)
        {
            var tempList = attacksUsedInString;
            
            for (int i = 0; i < attacksUsedInString.Count; i++)
            {
                if(attacksUsedInString[i].moveset == moveset
                    && attacksUsedInString[i].attack == attack)
                {
                    var tempItem = tempList[i];
                    tempItem.timesUsed++;
                    tempList[i] = tempItem;
                    return;
                }
            }

            tempList.Add(new MovesetAttackStringIdentifier()
            {
                moveset = moveset,
                attack = attack,
                timesUsed = 1
            });
        }

        public void ResetStringAttacks()
        {
            attacksUsedInString.Clear();
        }

        public virtual void CLateUpdate()
        {

        }

        public virtual FighterStats GetCurrentStats()
        {
            return (movesets[CurrentMovesetIdentifier] as Moveset).fighterStats;
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
            if (CurrentAttackNode == null)
            {
                return;
            }
            CurrentChargeLevel = 0;
            CurrentChargeLevelCharge = 0;
            CurrentAttackMovesetIdentifier = -1;
            CurrentAttackNodeIdentifier = -1;
            hitboxManager.Reset();
        }

        public int TryAttack()
        {
            if (CurrentMoveset == null)
            {
                return -1;
            }
            // We are currently not doing an attack, so check the starting nodes instead.
            if (CurrentAttackNode == null)
            {
                return CheckStartingNodes();
            }
            // We are doing an attack, check it's cancel windows.
            return CheckCurrentAttackCancelWindows();
        }

        protected virtual int CheckStartingNodes()
        {
            rwby.Moveset moveset = (Moveset)CurrentMoveset;
            switch (physicsManager.IsGrounded)
            {
                case true:
                    int groundAbility = CheckAbilityNodes(ref moveset.groundAbilityNodes);
                    if (groundAbility != -1)
                    {
                        return groundAbility;
                    }
                    if (moveset.groundIdleCancelListID != -1)
                    {
                        int cancel = TryCancelList(moveset.groundIdleCancelListID);
                        if (cancel != -1)
                        {
                            return cancel;
                        }
                    }
                    int groundNormal = CheckAttackNodes(ref moveset.groundAttackStartNodes);
                    if (groundNormal != -1)
                    {
                        return groundNormal;
                    }
                    break;
                case false:
                    int airAbility = CheckAbilityNodes(ref moveset.airAbilityNodes);
                    if (airAbility != -1)
                    {
                        return airAbility;
                    }
                    if (moveset.airIdleCancelListID != -1)
                    {
                        int cancel = TryCancelList(moveset.airIdleCancelListID);
                        if (cancel != -1)
                        {
                            return cancel;
                        }
                    }
                    int airNormal = CheckAttackNodes(ref moveset.airAttackStartNodes);
                    if (airNormal != -1)
                    {
                        return airNormal;
                    }
                    break;
            }
            return -1;
        }

        /// <summary>
        /// Checks the cancel windows of the current attack to see if we should transition to the next attack.
        /// </summary>
        /// <returns>The identifier of the attack if the transition conditions are met. Otherwise -1.</returns>
        protected virtual int CheckCurrentAttackCancelWindows()
        {
            // Our current moveset is not the same as our current attack's, ignore it's cancel windows.
            if (CurrentMoveset != CurrentAttackMoveset)
            {
                return -1;
            }
            for (int i = 0; i < CurrentAttackNode.nextNode.Count; i++)
            {
                if (stateManager.CurrentStateFrame >= CurrentAttackNode.nextNode[i].cancelWindow.x &&
                    stateManager.CurrentStateFrame <= CurrentAttackNode.nextNode[i].cancelWindow.y)
                {
                    MovesetAttackNode currentNode = (MovesetAttackNode)CurrentAttackNode;
                    MovesetAttackNode man = (MovesetAttackNode)CheckAttackNode((MovesetAttackNode)GetMoveset(CurrentAttackMovesetIdentifier).GetAttackNode(currentNode.nextNode[i].nodeIdentifier));
                    if (man != null)
                    {
                        return currentNode.nextNode[i].nodeIdentifier;
                    }
                }
            }
            return -1;
        }

        protected virtual int CheckAttackNodes<T>(ref List<T> nodes) where T : HnSF.Combat.MovesetAttackNode
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                HnSF.Combat.MovesetAttackNode man = CheckAttackNode(nodes[i]);
                if (man != null)
                {
                    return nodes[i].Identifier;
                }
            }
            return -1;
        }


        protected virtual HnSF.Combat.MovesetAttackNode CheckAttackNode(HnSF.Combat.MovesetAttackNode node)
        {
            if (node.attackDefinition == null)
            {
                return null;
            }

            if (CheckAttackNodeConditions(node) == false)
            {
                return null;
            }

            if (CheckForInputSequence(node.inputSequence))
            {
                return node;
            }
            return null;
        }

        [SerializeField] protected List<PlayerInputType> abilityButtons = new List<PlayerInputType>();
        protected virtual int CheckAbilityNodes<T>(ref List<T> nodes) where T : HnSF.Combat.MovesetAttackNode
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                HnSF.Combat.MovesetAttackNode man = CheckAbilityNode(abilityButtons[i], nodes[i]);
                if (man != null)
                {
                    return nodes[i].Identifier;
                }
            }
            return -1;
        }

        protected virtual HnSF.Combat.MovesetAttackNode CheckAbilityNode(PlayerInputType inputT, HnSF.Combat.MovesetAttackNode node)
        {
            if (node.attackDefinition == null)
            {
                return null;
            }

            if (CheckAttackNodeConditions(node) == false)
            {
                return null;
            }

            node.inputSequence.executeInputs[0].buttonID = (int)inputT;

            /*if (manager.InputManager.GetButton(inputT, out int bOff, 0, 5).firstPress == false)
            {
                return null;
            }*/

            if (CheckForInputSequence(node.inputSequence, 0, true))
            {
                //node.inputSequence.executeInputs[0].buttonID = (int)inputT;
                return node;
            }
            return null;
        }

        protected virtual bool CheckAttackNodeConditions(HnSF.Combat.MovesetAttackNode node)
        {
            if (node.Identifier == CurrentAttackNodeIdentifier) return false;
            if (StringAttackValid((byte)CurrentMovesetIdentifier, (byte)node.Identifier, (node as MovesetAttackNode).maxRepeatsInString) == false) return false;
            return true;
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

        /// <summary>
        /// Resets anything having to do with the current attack and sets a new one. This method assumes that the attack node is in the current moveset.
        /// </summary>
        /// <param name="attackNodeIdentifier">The identifier of the attack node.</param>
        public virtual void SetAttack(int attackNodeIdentifier)
        {
            Cleanup();
            CurrentAttackMovesetIdentifier = CurrentMovesetIdentifier;
            CurrentAttackNodeIdentifier = attackNodeIdentifier;
        }

        /// <summary>
        /// Resets anything having to do with the current attack and sets a new one. 
        /// </summary>
        /// <param name="attackNodeIdentifier"></param>
        /// <param name="attackMovesetIdentifier"></param>
        public virtual void SetAttack(int attackNodeIdentifier, int attackMovesetIdentifier)
        {
            Cleanup();
            CurrentAttackMovesetIdentifier = attackMovesetIdentifier;
            CurrentAttackNodeIdentifier = attackNodeIdentifier;
        }

        public int GetMovesetCount()
        {
            return movesets.Length;
        }

        public int GetTeam()
        {
            return Team;
        }

        public void SetMoveset(int movesetIdentifier)
        {
            CurrentMovesetIdentifier = movesetIdentifier;
        }

        public rwby.Moveset GetMoveset()
        {
            return (rwby.Moveset)movesets[CurrentMovesetIdentifier];
        }
        
        public MovesetDefinition GetMoveset(int identifier)
        {

            return movesets[identifier];
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

        /// <summary>
        /// Check a cancel list in the current moveset to see if an attack can be canceled into.
        /// </summary>
        /// <param name="cancelListID"></param>
        /// <returns>The identifier of the attack node if found. Otherwise -1.</returns>
        public virtual int TryCancelList(int cancelListID)
        {
            CancelList cl = CurrentMoveset.GetCancelList(cancelListID);
            if (cl == null)
            {
                return -1;
            }
            int attack = CheckAttackNodes(ref cl.nodes);
            if (attack != -1)
            {
                return attack;
            }
            return -1;
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

        public float autoLinkForcePercentage = 0;
        [Networked, Capacity(10)] public NetworkArray<int> hurtboxHitCount { get; }
        
        [Networked] public NetworkBool HardKnockdown { get; set; }
        [Networked] public NetworkBool GroundBounce { get; set; }

        public HitReactionBase Hurt(HurtInfoBase hurtInfoBase)
        {
            HurtInfo hurtInfo = hurtInfoBase as HurtInfo;
            HitInfo hitInfo = hurtInfo.hitInfo as HitInfo;
            HitReaction hitReaction = new HitReaction();
            hitReaction.reaction = HitReactionType.AVOIDED;

            /*
            int indexOfHurtboxGroup = hurtboxManager.GetHurtboxDefinition().hurtboxGroups.IndexOf(hurtInfo.hurtboxGroupHit);
            hurtboxHitCount.Set(indexOfHurtboxGroup, hurtboxHitCount[indexOfHurtboxGroup] + 1);*/

            // Check if the box can hit this entity.
            if (hitInfo.groundOnly && !physicsManager.IsGrounded
                || hitInfo.airOnly && physicsManager.IsGrounded)
            {
                return hitReaction;
            }

            if(BlockState != BlockStateType.NONE)
            {
                if(Vector3.Angle(transform.forward, hurtInfo.forward) > 90)
                {
                    hitReaction.reaction = HitReactionType.BLOCKED;
                    SetHitStop(hitInfo.blockHitstopDefender);
                    BlockStun = hitInfo.blockstun;

                    Vector3 baseBlockForce = manager.FPhysicsManager.IsGrounded ? hitInfo.blockForce : hitInfo.blockForceAir;
                    Vector3 blockForces = (baseBlockForce.x * hurtInfo.right) + (baseBlockForce.z * hurtInfo.forward);
                    physicsManager.forceGravity = baseBlockForce.y;
                    physicsManager.forceMovement = blockForces;
                    return hitReaction;
                }
            }

            manager.FPhysicsManager.SetRotation((hurtInfo.forward * -1).normalized);
            // Got hit, apply stun, damage, and forces.
            hitReaction.reaction = HitReactionType.HIT;
            SetHitStop(hitInfo.hitstop);
            SetHitStun(hitInfo.hitstun);

            Vector3 baseForce = manager.FPhysicsManager.IsGrounded ? hitInfo.opponentForce : hitInfo.opponentForceAir;
            hitstunGravityHangTime = hitInfo.hangTime;
            hitstunHoldVelocityTime = hitInfo.holdVelocityTime;
            hitstunFriction = hitInfo.opponentFriction;
            
            if (hitstunFriction <= -1)
            {
                hitstunFriction = manager.StatManager.GroundFriction;
            }
            hitstunGravity = hitInfo.opponentGravity;
            if (hitstunGravity <= -1)
            {
                hitstunGravity = manager.StatManager.hitstunGravity;
            }

            // Convert forces the attacker-based forward direction.
            switch (hitInfo.forceType)
            {
                case HitboxForceType.SET:
                    Vector3 forces = (baseForce.x * hurtInfo.right) + (baseForce.z * hurtInfo.forward);
                    physicsManager.forceGravity = baseForce.y;
                    physicsManager.forceMovement = forces;
                    break;
                case HitboxForceType.PULL:
                    Vector3 pullDir = Vector3.ClampMagnitude((hurtInfo.center - (transform.position + Vector3.up)) * hitInfo.opponentForceMultiplier, hitInfo.opponentMaxMagnitude);
                    if (pullDir.magnitude < hitInfo.opponentMinMagnitude)
                    {
                        pullDir = (hurtInfo.center - (transform.position + Vector3.up) ).normalized * hitInfo.opponentMinMagnitude;
                    }
                    if (hitInfo.forceIncludeYForce)
                    {
                        physicsManager.forceGravity = pullDir.y;
                    }
                    pullDir.y = 0;
                    physicsManager.forceMovement = pullDir;
                    break;
            }

            if (hitInfo.autoLink)
            {
                //autoLinkForcePercentage = hitInfo.autoLinkPercentage;
                Vector3 calcForce = hurtInfo.attackerVelocity * hitInfo.autoLinkPercentage;
                physicsManager.forceGravity += calcForce.y;
                calcForce.y = 0;
                physicsManager.forceMovement += calcForce;
                /*Vector3 temp = physicsManager.forceMovement;
                temp = (temp * (1.0f - autoLinkForcePercentage)) + (hurtInfo.attackerVelocity * autoLinkForcePercentage);
                temp.y = 0;
                physicsManager.forceMovement = temp;
                physicsManager.forceGravity = (physicsManager.forceGravity * (1.0f - autoLinkForcePercentage)) + (hurtInfo.attackerVelocity.y * autoLinkForcePercentage);*/
            }
            if (physicsManager.forceGravity > 0)
            {
                physicsManager.SetGrounded(false);
            }

            HardKnockdown = hitInfo.hardKnockdown;
            GroundBounce = hitInfo.groundBounces;

            if (physicsManager.IsGrounded == true)
            {
                if (hitInfo.causesTrip)
                {
                    manager.FStateManager.ChangeState((int)FighterCmnStates.TRIP);
                }
                else
                {
                    manager.FStateManager.ChangeState((int)FighterCmnStates.FLINCH_GROUND);
                }
            }
            else
            {
                if (hitInfo.forcesRestand)
                {
                    manager.FStateManager.ChangeState((int)FighterCmnStates.FLINCH_AIR);
                }
                else
                {
                    manager.FStateManager.ChangeState((int)FighterCmnStates.TUMBLE);
                }
            }
            manager.FCombatManager.Cleanup();
            return hitReaction;
        }
    }
}