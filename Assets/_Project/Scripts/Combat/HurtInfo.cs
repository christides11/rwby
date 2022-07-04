using HnSF.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class HurtInfo : HnSF.Combat.HurtInfo3D
    {
        public int hurtboxHit;
        public Vector3 attackerVelocity;
        public Vector3 hitPosition;
        public Vector3 hitboxOffset;
        
        public HurtInfo(HitInfo hitInfo, int hurtboxHit, Vector3 center, Vector3 forward, Vector3 right, Vector3 attackerVelocity, Vector3 hitPosition)
            : base(hitInfo, center, forward, right)
        {
            this.attackerVelocity = attackerVelocity;
            this.hurtboxHit = hurtboxHit;
            this.hitPosition = hitPosition;
        }
    }
}