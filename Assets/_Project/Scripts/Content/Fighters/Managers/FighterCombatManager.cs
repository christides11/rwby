using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using HnSF.Combat;
using HnSF.Fighters;
using HnSF.Input;
using System;

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

        public FighterHitboxManager HitboxManager { get { return hitboxManager; } }

        [SerializeField] protected HealthManager healthManager;
        [SerializeField] protected FighterHitboxManager hitboxManager;
        [SerializeField] protected FighterManager manager;
        [SerializeField] protected FighterInputManager inputManager;
        [SerializeField] protected FighterPhysicsManager physicsManager;
        [SerializeField] protected FighterStateManager stateManager;
        [SerializeField] protected FighterHurtboxManager hurtboxManager;

        [Networked] public int Team { get; set; }

        public MovesetDefinition[] movesets;

        [Networked] public int hitstunGravityHangTime { get; set; }
        [Networked] public float hitstunGravityHangTimer { get; set; }
        [Networked] public float hitstunHoldVelocityTime { get; set; }
        [Networked] public float hitstunFriction { get; set; }
        [Networked] public float hitstunGravity { get; set; }

        public virtual void CLateUpdate()
        {

        }

        public virtual void SetHitStop(int value)
        {
            HitStop = value;
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
            MovesetDefinition moveset = CurrentMoveset;
            switch (physicsManager.IsGrounded)
            {
                case true:
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

        protected virtual bool CheckAttackNodeConditions(HnSF.Combat.MovesetAttackNode node)
        {
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

        public MovesetDefinition GetMoveset(int identifier)
        {
            /*
            if(identifier == 0)
            {
                return null;
            }*/
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

        protected virtual bool CheckStickDirection(HnSF.Input.InputDefinition sequenceInput, int framesBack)
        {
            Vector2 stickDir = inputManager.GetMovement(framesBack);
            if (stickDir.magnitude < InputConstants.movementDeadzone)
            {
                return false;
            }

            if (Vector2.Dot(stickDir, sequenceInput.stickDirection) >= sequenceInput.directionDeviation)
            {
                return true;
            }
            return false;
        }

        protected virtual void ClearBuffer()
        {
            inputManager.BufferLimit = Runner.Simulation.Tick;
        }

        public void Heal(HealInfoBase healInfo)
        {
            throw new System.NotImplementedException();
        }

        public float autoLinkForcePercentage = 0.85f;
        [Networked, Capacity(10)] public NetworkArray<int> hurtboxHitCount { get; set; }
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

            
            if (BlockState != BlockStateType.NONE)
            {
                if (hitInfo.blockType == HitBlockType.MID)
                {
                    hitReaction.reaction = HitReactionType.BLOCKED;
                    BlockStun = hitInfo.blockstun;
                    return hitReaction;
                }
                else if (BlockState == BlockStateType.HIGH && hitInfo.blockType == HitBlockType.HIGH)
                {
                    hitReaction.reaction = HitReactionType.BLOCKED;
                    BlockStun = hitInfo.blockstun;
                    return hitReaction;
                }
                else if (BlockState == BlockStateType.LOW && hitInfo.blockType == HitBlockType.LOW)
                {
                    hitReaction.reaction = HitReactionType.BLOCKED;
                    BlockStun = hitInfo.blockstun;
                    return hitReaction;
                }
            }

            // Got hit, apply stun, damage, and forces.
            hitReaction.reaction = HitReactionType.HIT;
            SetHitStop(hitInfo.hitstop);
            SetHitStun(hitInfo.hitstun);

            Vector3 baseForce = manager.PhysicsManager.IsGrounded ? hitInfo.opponentForce : hitInfo.opponentForceAir;
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
            }

            if (hitInfo.autoLink)
            {
                Vector3 temp = physicsManager.forceMovement;
                temp.x += hurtInfo.attackerVelocity.x * autoLinkForcePercentage;
                temp.z += hurtInfo.attackerVelocity.z * autoLinkForcePercentage;
                physicsManager.forceMovement = temp;
            }
            if (physicsManager.forceGravity > 0)
            {
                physicsManager.SetGrounded(false);
            }

            if(manager.StateManager.CurrentState == (ushort)FighterCmnStates.ATTACK)
            {
                manager.CombatManager.Cleanup();
            }
            // Change into the correct state.
            if (hitInfo.groundBounces && physicsManager.IsGrounded)
            {
                manager.StateManager.ChangeState((int)FighterCmnStates.GROUND_BOUNCE);
            }
            else if (hitInfo.causesTumble)
            {
                manager.StateManager.ChangeState((int)FighterCmnStates.TUMBLE);
            }
            else
            {
                manager.StateManager.ChangeState((ushort)(physicsManager.IsGrounded ? FighterCmnStates.FLINCH_GROUND : FighterCmnStates.FLINCH_AIR));
            }
            return hitReaction;
        }
    }
}