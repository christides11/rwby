using HnSF.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class Hurtbox : Fusion.Hitbox
    {
        public IHurtable hurtable;
        public HurtboxGroup hurtboxGroup;
    }
}