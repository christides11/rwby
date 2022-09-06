using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace rwby
{
    public class CRBaseProjectile : BaseProjectile
    {
        [Networked] public TickTimer timer { get; set; }
        public int ticksToExist = 200;

        public VarCreateBox box;
        
        private void Awake()
        {
            boxManager.hurtable = this;
        }

        public override void Spawned()
        {
            base.Spawned();
            timer = TickTimer.CreateFromTicks(Runner, ticksToExist);
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            if (timer.Expired(Runner))
            {
                Runner.Despawn(Object, true);
                return;
            }
            boxManager.AddBox(box.boxType, box.attachedTo, box.shape, box.offset, box.boxExtents, box.radius,
                box.definitionIndex, this);
        }
    }
}