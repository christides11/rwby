using System;
using Fusion;
using HnSF.Combat;
using UnityEngine;

namespace rwby
{
    public class BaseProjectile : NetworkBehaviour, IBoxDefinitionCollection, IAttacker, IHurtable
    {
        [NonSerialized] public int bank;
        [NonSerialized] public int projectile;
        
        [Networked, Capacity(5)] public NetworkLinkedList<IDGroupCollisionInfo> hitObjects => default;
        [Networked] public NetworkObject owner { get; set; }
        [Networked] public int team { get; set; }
        [Networked] public Vector3 force { get; set; }

        public HitInfo[] HitboxInfo
        {
            get { return hitboxInfo; }
        }
        public ThrowInfo[] ThrowboxInfo
        {
            get { return throwboxInfo; }
        }
        public HurtboxInfo[] HurtboxInfo
        {
            get { return hurtboxInfo; }
        }

        public EntityBoxManager boxManager;
        
        [Header("BoxDefinitions")]
        [SerializeField] private HitInfo[] hitboxInfo;
        [SerializeField] private ThrowInfo[] throwboxInfo;
        [SerializeField] private HurtboxInfo[] hurtboxInfo;

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            boxManager.ResetAllBoxes();
            Move();
        }

        public virtual void Move()
        {
            transform.position += force * Runner.DeltaTime;
        }

        public bool IsHitHurtboxValid(CustomHitbox atackerHitbox, Hurtbox h)
        {
            if (h.ownerNetworkObject == Object || h.ownerNetworkObject == owner) return false;
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

        public bool IsHitHitboxValid(CustomHitbox attackerHitbox, CustomHitbox h)
        {
            if (h.ownerNetworkObject == Object || h.ownerNetworkObject == owner) return false;
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

        public HurtInfo BuildHurtInfo(CustomHitbox hitbox, Hurtbox hurtbox)
        {
            Vector3 hitPoint = hurtbox.transform.position;
            HitInfo hitInfo = hitbox.definition.HitboxInfo[hitbox.definitionIndex];
            HurtInfo hurtInfo;
            
            hurtInfo = new HurtInfo(hitInfo, hurtbox.definitionIndex,
                transform.position, transform.forward, transform.right,
                Vector3.zero, hitPoint);
            return hurtInfo;
        }

        public void DoHit(CustomHitbox hitbox, Hurtbox enemyHurtbox, HurtInfo hurtInfo)
        {
            hitObjects.Add(new IDGroupCollisionInfo()
            {
                collisionType = IDGroupCollisionType.Hurtbox,
                hitByIDGroup = hitbox.definition.HitboxInfo[hitbox.definitionIndex].ID,
                hitIHurtableNetID = enemyHurtbox.ownerNetworkObject.Id
            });

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
            hitObjects.Add(new IDGroupCollisionInfo()
            {
                collisionType = IDGroupCollisionType.Hitbox,
                hitByIDGroup = hitbox.definition.HitboxInfo[hitbox.definitionIndex].ID,
                hitIHurtableNetID = enemyHitbox.ownerNetworkObject.Id
            });
            
            //TODO: Clashing.
        }

        public int GetTeam()
        {
            return team;
        }

        public HitReactionBase Hurt(HurtInfoBase hurtInfo)
        {
            HitReaction hr = new HitReaction();
            return hr;
        }
    }
}