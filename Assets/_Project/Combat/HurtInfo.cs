using HnSF.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class HurtInfo : HnSF.Combat.HurtInfo3D
    {
        public HurtboxGroup hurtboxGroupHit;
        public Vector3 attackerVelocity;

        public HurtInfo(HitInfo hitInfo, HurtboxGroup hurtboxGroupHit, Vector3 center, Vector3 forward, Vector3 right, Vector3 attackerVelocity)
            : base(hitInfo, center, forward, right)
        {
            this.attackerVelocity = attackerVelocity;
            this.hurtboxGroupHit = hurtboxGroupHit;
        }
    }
}