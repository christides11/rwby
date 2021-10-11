using HnSF.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace rwby
{
    public class Hurtbox : Fusion.Hitbox
    {
        public NetworkObject hurtableNetworkObject;
        public IHurtable hurtable;
        public HurtboxGroup hurtboxGroup;
    }
}