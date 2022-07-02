using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace rwby
{
    public class ProjectileBase : NetworkBehaviour, IBoxDefinitionCollection, IAttacker
    {
        [Networked] public NetworkObject owner { get; set; }
        
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
        
        public bool IsHitHurtboxValid(CustomHitbox atackerHitbox, Hurtbox h)
        {
            throw new System.NotImplementedException();
        }

        public bool IsHitHitboxValid(CustomHitbox attackerHitbox, CustomHitbox h)
        {
            throw new System.NotImplementedException();
        }

        public HurtInfo BuildHurtInfo(CustomHitbox hitbox, Hurtbox enemyHurtbox)
        {
            throw new System.NotImplementedException();
        }

        public void DoHit(CustomHitbox hitbox, Hurtbox enemyHurtbox, HurtInfo hurtInfo)
        {
            throw new System.NotImplementedException();
        }

        public void DoClash(CustomHitbox hitbox, CustomHitbox enemyHitbox)
        {
            throw new System.NotImplementedException();
        }
    }
}