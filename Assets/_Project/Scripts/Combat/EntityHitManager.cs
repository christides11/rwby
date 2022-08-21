using Fusion;
using HnSF.Combat;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class EntityHitManager : NetworkBehaviour, IAttacker, IThrower, IThrowee
    {
        protected GameManager gameManager;

        public Transform myTransform;

        public LayerMask hitLayermask;
        public LayerMask grabLayermask;
        public List<LagCompensatedHit> lch = new List<LagCompensatedHit>();

        [Networked, Capacity(10)] public NetworkLinkedList<IDGroupCollisionInfo> hitObjects { get; }

        protected virtual void Awake()
        {
            gameManager = GameManager.singleton;
        }

        public virtual void Reset()
        {
            hitObjects.Clear();
            for (int i = 0; i < hitboxGroupHitCounts.Length; i++)
            {
                hitboxGroupHitCounts.Set(i, 0);
                hitboxGroupBlockedCounts.Set(i, 0);
            }
        }

        public virtual bool IsHitHurtboxValid(CustomHitbox atackerHitbox, Hurtbox h)
        {
            if (h.ownerNetworkObject == Object) return false;
            for(int i = 0; i < hitObjects.Count; i++)
            {
                if(hitObjects[i].collisionType == IDGroupCollisionType.Hurtbox
                    && hitObjects[i].hitIHurtableNetID == h.ownerNetworkObject.Id
                    && hitObjects[i].hitByIDGroup == atackerHitbox.definition.HitboxInfo[atackerHitbox.definitionIndex].ID)
                {
                    return false;
                }
            }
            return true;
        }

        public virtual bool IsHitHitboxValid(CustomHitbox attackerHitbox, CustomHitbox h)
        {
            if (h.ownerNetworkObject == Object) return false;
            for (int i = 0; i < hitObjects.Count; i++)
            {
                if (hitObjects[i].collisionType == IDGroupCollisionType.Hitbox
                    && hitObjects[i].hitIHurtableNetID == h.ownerNetworkObject.Id
                    && hitObjects[i].hitByIDGroup == attackerHitbox.definition.HitboxInfo[attackerHitbox.definitionIndex].ID)
                {
                    return false;
                }
            }
            return true;
        }

        public virtual bool IsThroweeValid(CustomHitbox attackerThrowbox, Throwablebox attackeeThrowablebox)
        {
            if (attackeeThrowablebox.ownerNetworkObject == Object) return false;
            return true;
        }

        public virtual void ThrowerInitilization(NetworkObject throwee)
        {
            
        }

        public virtual void ThroweeInitilization(NetworkObject thrower)
        {
            
        }

        public virtual void SetThroweePosition(Vector3 position)
        {
            
        }

        public virtual void SetThroweeRotation(Vector3 rotation)
        {
            
        }
        
        [Networked, Capacity(10)] public NetworkArray<int> hitboxGroupHitCounts { get; }
        [Networked, Capacity(10)] public NetworkArray<int> hitboxGroupBlockedCounts { get; }
        public virtual void DoHit(CustomHitbox hitbox, Hurtbox enemyHurtbox, HurtInfo hurtInfo)
        {
            hitObjects.Add(new IDGroupCollisionInfo()
            {
                collisionType = IDGroupCollisionType.Hurtbox,
                hitByIDGroup = hitbox.definition.HitboxInfo[hitbox.definitionIndex].ID,
                hitIHurtableNetID = enemyHurtbox.ownerNetworkObject.Id
            });

            HitReaction reaction = (HitReaction)enemyHurtbox.hurtable.Hurt(hurtInfo);

            HitInfo hi = hurtInfo.hitInfo as HitInfo;
            
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
            }
        }

        public virtual void HandleHitReaction(CustomHitbox hitbox, Hurtbox enemyHurtbox, HurtInfo hurtInfo, HitInfo hi,
            HitReaction hitReaction)
        {
            hitboxGroupHitCounts.Set(hitbox.definitionIndex, hitboxGroupHitCounts[hitbox.definitionIndex] + 1);
        }

        public virtual void HandleBlockReaction(CustomHitbox hitbox, Hurtbox enemyHurtbox, HurtInfo hurtInfo,
            HitInfo hi, HitReaction hitReaction)
        {
            hitboxGroupBlockedCounts.Set(hitbox.definitionIndex, hitboxGroupBlockedCounts[hitbox.definitionIndex] + 1);
        }
        
        public virtual void HandleAvoidReaction(CustomHitbox hitbox, Hurtbox enemyHurtbox, HurtInfo hurtInfo,
            HitInfo hi, HitReaction hitReaction)
        {
            
        }

        public virtual void DoClash(CustomHitbox hitbox, CustomHitbox enemyHitbox)
        {
            hitObjects.Add(new IDGroupCollisionInfo()
            {
                collisionType = IDGroupCollisionType.Hitbox,
                hitByIDGroup = hitbox.definition.HitboxInfo[hitbox.definitionIndex].ID,
                hitIHurtableNetID = enemyHitbox.ownerNetworkObject.Id
            });
        }

        public virtual HurtInfo BuildHurtInfo(CustomHitbox hitbox, Hurtbox hurtbox)
        {
            Vector3 hitPoint = hurtbox.transform.position;
            HitInfo hitInfo = hitbox.definition.HitboxInfo[hitbox.definitionIndex];
            HurtInfo hurtInfo;

            hurtInfo = new HurtInfo(hitInfo, hurtbox.definitionIndex,
                myTransform.position, myTransform.forward, myTransform.right,
                Vector3.zero, hitPoint);
            return hurtInfo;
        }
    }
}
