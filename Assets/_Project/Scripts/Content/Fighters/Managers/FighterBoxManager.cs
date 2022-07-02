using Fusion;
using HnSF.Combat;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    [OrderAfter(typeof(Fusion.HitboxManager))]
    public class FighterBoxManager : EntityBoxManager
    {
        public override void Awake()
        {
            hurtable = GetComponent<IHurtable>();
            base.Awake();
        }
    }
}