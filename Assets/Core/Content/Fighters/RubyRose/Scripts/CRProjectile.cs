using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace rwby
{
    /*
    public class CRProjectile : ProjectileBase
    {
        
        [Networked] public TickTimer timer { get; set; }
        public int ticksToExist = 200;

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
            }
        }
    }*/
}