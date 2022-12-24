using System.Collections;
using System.Collections.Generic;
using Fusion;
using HnSF;
using HnSF.Combat;
using UnityEngine;

namespace rwby
{
    public class DeathZone : NetworkBehaviour, IBoxDefinitionCollection, IAttacker, IHurtable, ITeamable
    {
        public HitInfo[] HitboxInfo
        {
            get { return hitboxInfo; }
        }
        public ThrowInfo[] ThrowboxInfo
        {
            get { return null; }
        }
        public HurtboxInfo[] HurtboxInfo
        {
            get { return null; }
        }
        
        public EntityBoxManager boxManager;
        public VarCreateBox cb;
        
        [Header("BoxDefinition")]
        [SerializeField] private HitInfo[] hitboxInfo;

        public override void Spawned()
        {
            base.Spawned();
            boxManager.AddBox(cb.boxType, cb.attachedTo, cb.shape, cb.offset, cb.boxExtents, cb.radius, cb.definitionIndex, this);
        }

        public override void FixedUpdateNetwork()
        {
            //boxManager.ResetAllBoxes();
            //boxManager.AddBox(cb.boxType, cb.attachedTo, cb.shape, cb.offset, cb.boxExtents, cb.radius, cb.definitionIndex, this);
        }

        public bool IsHitHurtboxValid(CustomHitbox atackerHitbox, Hurtbox h)
        {
            return true;
        }

        public bool IsHitHitboxValid(CustomHitbox attackerHitbox, CustomHitbox h)
        {
            return false;
        }

        public HurtInfo BuildHurtInfo(CustomHitbox hitbox, Hurtbox enemyHurtbox)
        {
            Vector3 hitPoint = enemyHurtbox.transform.position;
            HitInfo hitInfo = this.HitboxInfo[hitbox.definitionIndex];

            var hurtInfo = new HurtInfo(hitInfo, enemyHurtbox.definitionIndex,
                transform.position, transform.forward, transform.right,
                Vector3.zero, hitPoint);
            return hurtInfo;
        }

        public void DoHit(CustomHitbox hitbox, Hurtbox enemyHurtbox, HurtInfo hurtInfo)
        {
            HitReaction reaction = (HitReaction)enemyHurtbox.hurtable.Hurt(hurtInfo);

            HitInfo hi = hurtInfo.hitInfo as HitInfo;
            
            /*
            switch (reaction.reaction)
            {
                case HitReactionType.HIT:
                    HandleHitReaction(hitbox, enemyHurtbox, hurtInfo, hi, reaction);
                    break;
                case HitReactionType.BLOCKED:
                    HandleBlockReaction(hitbox, enemyHurtbox, hurtInfo, hi, reaction);
                    break;
                case HitReactionType.AVOIDED:
                    HandleAvoidReaction(hitbox, enemyHurtbox, hurtInfo, hi, reaction);
                    break;
            }*/
        }

        public void DoClash(CustomHitbox hitbox, CustomHitbox enemyHitbox)
        {
            
        }

        public int GetTeam()
        {
            return -1;
        }

        public HitReactionBase Hurt(HurtInfoBase hurtInfo)
        {
            HitReaction hr = new HitReaction();
            return hr;
        }
    }
}