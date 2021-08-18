using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using HnSF.Combat;
using HnSF.Fighters;
using HnSF.Input;

namespace rwby
{
    public class FighterCombatManager : NetworkBehaviour, IHurtable, IFighterCombatManager
    {
        [Networked] public int HitStun { get; set; }
        [Networked] public int HitStop { get; set; }
        [Networked] public int CurrentChargeLevel { get; set; }
        [Networked] public int CurrentChargeLevelCharge { get; set; }
        [Networked] public int CurrentMovesetIdentifier { get; set; }
        /// <summary>
        /// The identifier of the moveset that the current attack belongs to. Not the same as our current moveset.
        /// </summary>
        [Networked] public int CurrentAttackMovesetIdentifier { get; set; }
        [Networked] public int CurrentAttackNodeIdentifier { get; set; }
        public MovesetDefinition CurrentMoveset { get; set; }
        /// <summary>
        /// The moveset that the current attack belongs to. Not the same as our current moveset.
        /// </summary>
        public MovesetDefinition CurrentAttackMoveset { get; set; }
        public MovesetAttackNode CurrentAttackNode { get; set; }

        [SerializeField] protected HealthManager healthManager;

        public MovesetDefinition[] movesets;

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

        public void Heal(HealInfoBase healInfo)
        {
            throw new System.NotImplementedException();
        }

        public HitReactionBase Hurt(HurtInfoBase hurtInfo)
        {
            throw new System.NotImplementedException();
        }

        public bool CheckForInputSequence(InputSequence sequence, uint baseOffset = 0, bool processSequenceButtons = false, bool holdInput = false)
        {
            throw new System.NotImplementedException();
        }

        public void Cleanup()
        {
            throw new System.NotImplementedException();
        }

        public MovesetDefinition GetMoveset(int index)
        {
            throw new System.NotImplementedException();
        }

        public int GetMovesetCount()
        {
            throw new System.NotImplementedException();
        }

        public int GetTeam()
        {
            throw new System.NotImplementedException();
        }

        public void IncrementChargeLevelCharge(int maxCharge)
        {
            throw new System.NotImplementedException();
        }

        public void SetAttack(int attackNodeIdentifier)
        {
            throw new System.NotImplementedException();
        }

        public void SetAttack(int attackNodeIdentifier, int attackMovesetIdentifier)
        {
            throw new System.NotImplementedException();
        }

        public void SetChargeLevel(int value)
        {
            throw new System.NotImplementedException();
        }

        public void SetChargeLevelCharge(int value)
        {
            throw new System.NotImplementedException();
        }

        public void SetMoveset(int movesetIdentifier)
        {
            CurrentMovesetIdentifier = movesetIdentifier;
        }

        public int TryAttack()
        {
            throw new System.NotImplementedException();
        }

        public int TryCancelList(int cancelListID)
        {
            throw new System.NotImplementedException();
        }
    }
}