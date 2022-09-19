using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace rwby
{
    public class CRBaseProjectile : BaseProjectile
    {
        [Networked] public int startTick { get; set; }
        [Networked] public TickTimer timer { get; set; }
        public int ticksToExist = 200;

        [Header("Movement")]
        public AnimationCurve forwardMoveCurve;
        public float fMoveForce;

        public AnimationCurve moveTowardsCurve;
        public float moveTowardsForce;
        
        private void Awake()
        {
            boxManager.hurtable = this;
        }

        public override void Spawned()
        {
            base.Spawned();
            timer = TickTimer.CreateFromTicks(Runner, ticksToExist);
            startTick = Runner.Tick;
        }

        public override void FixedUpdateNetwork()
        {
            if (timer.Expired(Runner))
            {
                Runner.Despawn(Object, true);
                return;
            }

            int cExistTick = Runner.Tick - startTick;

            float t = (float)(cExistTick) / (float)(ticksToExist);
            
            force = transform.forward * fMoveForce * forwardMoveCurve.Evaluate(t);

            var fm = owner.GetBehaviour<FighterManager>();
            var mF = Vector3.MoveTowards(transform.position, fm.GetCenter(), moveTowardsForce) - transform.position;

            force += (mF/Runner.DeltaTime) * moveTowardsCurve.Evaluate(t);
            if (cExistTick % 3 == 0)
            {
                Reset();
            }
            base.FixedUpdateNetwork();
        }
    }
}