using HnSF.Combat;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

namespace rwby
{
    public class SAttack : FighterState
    {
        public override string GetName()
        {
            AttackDefinition currentAttack = (AttackDefinition)manager.CombatManager.CurrentAttackNode.attackDefinition;
            return currentAttack.attackName;
        }

        public override void Initialize()
        {
            manager.CombatManager.Charging = true;
            manager.HurtboxManager.ResetHurtboxes();
            manager.CombatManager.HitboxManager.Reset();
            AttackDefinition currentAttack = (AttackDefinition)manager.CombatManager.CurrentAttackNode.attackDefinition;
            for (int i = 0; i < manager.attackEventInput.Length; i++)
            {
                manager.attackEventInput.Set(i, false);
            }
            if (currentAttack.useState)
            {
                manager.StateManager.GetState(currentAttack.stateOverride).Initialize();
            }
        }

        public override void OnUpdate()
        {
            AttackDefinition currentAttack = (AttackDefinition)manager.CombatManager.CurrentAttackNode.attackDefinition;
            manager.HurtboxManager.CreateHurtboxes(0, 0);

            if (currentAttack.useState)
            {
                manager.StateManager.GetState(currentAttack.stateOverride).OnUpdate();
            }

            if (TryCancelWindow(currentAttack))
            {
                return;
            }

            bool eventCancel = false;
            bool interrupted = false;
            bool cleanup = false;
            for (int i = 0; i < currentAttack.events.Count; i++)
            {
                if (currentAttack.events[i].CheckConditions(manager) == false)
                {
                    continue;
                }
                switch (HandleEvents(i, currentAttack, currentAttack.events[i]))
                {
                    case HnSF.Combat.AttackEventReturnType.STALL:
                        // Event wants us to stall on the current frame.
                        eventCancel = true;
                        break;
                    case HnSF.Combat.AttackEventReturnType.INTERRUPT:
                        interrupted = true;
                        cleanup = true;
                        break;
                    case HnSF.Combat.AttackEventReturnType.INTERRUPT_NO_CLEANUP:
                        interrupted = true;
                        break;
                }
                if (interrupted == true)
                {
                    break;
                }
            }

            if (interrupted)
            {
                if (cleanup)
                {
                    manager.CombatManager.Cleanup();
                }
                return;
            }
        }

        public override void OnLateUpdate()
        {
            AttackDefinition currentAttack = (AttackDefinition)manager.CombatManager.CurrentAttackNode.attackDefinition;

            if (manager.CombatManager.HitStop != 0)
            {
                return;
            }

            for (int i = 0; i < currentAttack.hitboxGroups.Count; i++)
            {
                HandleHitboxGroup(i, currentAttack.hitboxGroups[i]);
            }

            /*
            FighterManager entityManager = FighterManager;
            if (eventCancel == false && HandleChargeLevels(entityManager, currentAttack) == false)
            {
                entityManager.StateManager.IncrementFrame();
            }*/

            if (CheckInterrupt())
            {
                return;
            }
            
            if (HandleChargeLevels(manager, currentAttack) == false && currentAttack.useState == false)
            {
                manager.StateManager.IncrementFrame();
            }

            if (currentAttack.useState)
            {
                manager.StateManager.GetState(currentAttack.stateOverride).OnLateUpdate();
            }
        }

        protected virtual bool TryCancelWindow(AttackDefinition currentAttack)
        {
            FighterManager e = manager;
            for (int i = 0; i < currentAttack.cancels.Count; i++)
            {
                if (e.StateManager.CurrentStateFrame >= currentAttack.cancels[i].startFrame
                    && e.StateManager.CurrentStateFrame <= currentAttack.cancels[i].endFrame)
                {
                    int man = e.CombatManager.TryCancelList(currentAttack.cancels[i].cancelListID);
                    if (man != -1)
                    {
                        e.CombatManager.SetAttack(man);
                        e.StateManager.ChangeState((int)FighterCmnStates.ATTACK);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Handles the lifetime of events.
        /// </summary>
        /// <param name="currentEvent">The event being processed.</param>
        /// <returns>True if the current attack state was canceled by the event.</returns>
        protected virtual HnSF.Combat.AttackEventReturnType HandleEvents(int eventIndex, AttackDefinition currentAttack, HnSF.Combat.AttackEventDefinition currentEvent)
        {
            if (!currentEvent.active)
            {
                return HnSF.Combat.AttackEventReturnType.NONE;
            }

            if (manager.CombatManager.CurrentChargeLevel < currentEvent.chargeLevelMin
                || manager.CombatManager.CurrentChargeLevel > currentEvent.chargeLevelMax)
            {
                return HnSF.Combat.AttackEventReturnType.NONE;
            }

            // Input Checking.
            if (manager.StateManager.CurrentStateFrame >= currentEvent.inputCheckStartFrame
                && manager.StateManager.CurrentStateFrame <= currentEvent.inputCheckEndFrame)
            {
                switch (currentEvent.inputCheckTiming)
                {
                    case HnSF.Combat.AttackEventInputCheckTiming.ONCE:
                        if (manager.attackEventInput[eventIndex] == true)
                        {
                            break;
                        }
                        manager.attackEventInput.Set(eventIndex, manager.CombatManager.CheckForInputSequence(currentEvent.input));
                        break;
                    case HnSF.Combat.AttackEventInputCheckTiming.CONTINUOUS:
                        manager.attackEventInput.Set(eventIndex, manager.CombatManager.CheckForInputSequence(currentEvent.input, 0, true, true));
                        break;
                }
            }

            if (currentEvent.inputCheckTiming != HnSF.Combat.AttackEventInputCheckTiming.NONE
                && manager.attackEventInput[eventIndex] == false)
            {
                return HnSF.Combat.AttackEventReturnType.NONE;
            }


            if (manager.StateManager.CurrentStateFrame >= currentEvent.startFrame
                && manager.StateManager.CurrentStateFrame <= currentEvent.endFrame)
            {
                // Hit Check.
                /*if (currentEvent.onHitCheck != HnSF.Combat.OnHitType.NONE)
                {
                    if (currentEvent.onHitCheck == HnSF.Combat.OnHitType.ID_GROUP)
                    {
                        if (manager.CombatManager.HitboxManager.IDGroupHasHurt(currentEvent.onHitIDGroup) == false)
                        {
                            return HnSF.Combat.AttackEventReturnType.NONE;
                        }
                    }
                    else if (currentEvent.onHitCheck == HnSF.Combat.OnHitType.HITBOX_GROUP)
                    {
                        if (manager.CombatManager.HitboxManager.HitboxGroupHasHurt(currentAttack.hitboxGroups[currentEvent.onHitHitboxGroup].ID,
                            currentEvent.onHitHitboxGroup) == false)
                        {
                            return HnSF.Combat.AttackEventReturnType.NONE;
                        }
                    }
                }*/
                return currentEvent.attackEvent.Evaluate((int)(manager.StateManager.CurrentStateFrame - currentEvent.startFrame),
                    currentEvent.endFrame - currentEvent.startFrame,
                    manager);
            }
            return HnSF.Combat.AttackEventReturnType.NONE;
        }

        /// <summary>
        /// Handles the lifetime process of box groups.
        /// </summary>
        /// <param name="groupIndex">The group number being processed.</param>
        /// <param name="boxGroup">The group being processed.</param>
        protected virtual void HandleHitboxGroup(int groupIndex, HnSF.Combat.HitboxGroup boxGroup)
        {
            // Make sure we're in the frame window of the box.
            if (manager.StateManager.CurrentStateFrame-1 < boxGroup.activeFramesStart
                || manager.StateManager.CurrentStateFrame-1 > boxGroup.activeFramesEnd)
            {
                return;
            }

            // Check if the charge level requirement was met.
            if (boxGroup.chargeLevelNeeded >= 0)
            {
                int currentChargeLevel = manager.CombatManager.CurrentChargeLevel;
                if (currentChargeLevel < boxGroup.chargeLevelNeeded
                    || currentChargeLevel > boxGroup.chargeLevelMax)
                {
                    return;
                }
            }

            // Hit check.
            switch (boxGroup.hitboxHitInfo.hitType)
            {
                case HnSF.Combat.HitboxType.HIT:
                    bool hitResult = manager.CombatManager.HitboxManager.CheckForCollision(groupIndex, (HitboxGroup)boxGroup, manager.gameObject);
                    if (hitResult == true)
                    {
                        /*
                        if ((boxGroup.hitboxHitInfo as HitInfo).hitSound != null)
                        {
                            SimulationAudioManager.Play((boxGroup.hitboxHitInfo as HitInfo).hitSound, Manager.transform.position, AudioPlayMode.ROLLBACK);
                        }*/
                    }
                    break;
            }
        }

        /// <summary>
        /// Handles processing the charge levels of the current attack.
        /// </summary>
        /// <param name="entityManager">The entity itself.</param>
        /// <param name="currentAttack">The current attack the entity is doing.</param>
        /// <returns>If the frame should be held.</returns>
        private bool HandleChargeLevels(FighterManager entityManager, AttackDefinition currentAttack)
        {
            if(manager.CombatManager.Charging == false)
            {
                return false;
            }

            bool result = false;
            for (int i = 0; i < currentAttack.chargeWindows.Count; i++)
            {
                if (manager.StateManager.CurrentStateFrame != currentAttack.chargeWindows[i].startFrame)
                {
                    continue;
                }


            }
            return result;
            /*
            for (int i = 0; i < currentAttack.chargeWindows.Count; i++)
            {
                PlayerInputType button = (currentAttack.chargeWindows[i] as Mahou.Combat.ChargeDefinition).input;
                if (entityManager.InputManager.GetButton((int)button).isDown == false)
                {
                    entityManager.charging = false;
                    result = false;
                    break;
                }

                // Still have charge levels to go through.
                if (entityManager.CombatManager.CurrentChargeLevel < currentAttack.chargeWindows[i].chargeLevels.Count)
                {
                    cManager.IncrementChargeLevelCharge(currentAttack.chargeWindows[i].chargeLevels[cManager.CurrentChargeLevel].maxChargeFrames);
                    // Charge completed, move on to the next level.
                    if (cManager.CurrentChargeLevelCharge == currentAttack.chargeWindows[i].chargeLevels[cManager.CurrentChargeLevel].maxChargeFrames)
                    {
                        cManager.SetChargeLevel(cManager.CurrentChargeLevel + 1);
                        cManager.SetChargeLevelCharge(0);
                    }
                }
                else if (currentAttack.chargeWindows[i].releaseOnCompletion)
                {
                    entityManager.charging = false;
                }
                result = true;
                // Only one charge level can be handled per frame, ignore everything else.
                break;
            }
            return result;*/
        }

        public override bool CheckInterrupt()
        {
            AttackDefinition currentAttack = (AttackDefinition)manager.CombatManager.CurrentAttackNode.attackDefinition;
            if (currentAttack.useState && manager.StateManager.GetState(currentAttack.stateOverride).CheckInterrupt())
            {
                return true;
            }
            if (manager.TryAttack())
            {
                return true;
            }
            if (FirstInterruptableFrameCheck())
            {
                manager.CombatManager.Cleanup();
                return true;
            }
            if (manager.StateManager.CurrentStateFrame > manager.CombatManager.CurrentAttackNode.attackDefinition.length)
            {
                if (manager.PhysicsManager.IsGrounded)
                {
                    manager.StateManager.ChangeState((int)FighterCmnStates.IDLE);
                }
                else
                {
                    manager.StateManager.ChangeState((int)FighterCmnStates.FALL);
                }
                manager.CombatManager.Cleanup();
                return true;
            }
            return false;
        }

        public override void OnInterrupted()
        {
            AttackDefinition currentAttack = (AttackDefinition)manager.CombatManager.CurrentAttackNode.attackDefinition;
            if (currentAttack.useState)
            {
                manager.StateManager.GetState(currentAttack.stateOverride).OnInterrupted();
            }
        }

        private bool FirstInterruptableFrameCheck()
        {
            AttackDefinition currentAttack = (AttackDefinition)manager.CombatManager.CurrentAttackNode.attackDefinition;
            if (manager.StateManager.CurrentStateFrame < currentAttack.firstActableFrame)
            {
                return false;
            }

            if (manager.TryJump())
            {
                return true;
            }
            Vector2 move = manager.InputManager.GetMovement(0);
            if (move.magnitude >= InputConstants.movementDeadzone)
            {
                manager.StateManager.ChangeState((int)FighterCmnStates.WALK);
                return true;
            }
            return false;
        }
    }
}