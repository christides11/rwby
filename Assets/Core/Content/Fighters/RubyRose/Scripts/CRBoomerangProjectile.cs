using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    public class CRBoomerangProjectile : BaseProjectile
    {
        [Networked] public int startTick { get; set; }
        [Networked] public TickTimer timer { get; set; }
        public int ticksToExist = 200;

        [FormerlySerializedAs("forwardMoveCurve")] [Header("Movement")]
        public AnimationCurve moveCurve;
        public float fMoveForce;

        public int clearPer = 2;

        public NetworkObject crPrefab;

        //public float destroyDistance;
        //public float destroyDistAfter = 0.5f;
        
        private void Awake()
        {
            boxManager.hurtable = this;
        }

        public override void Spawned()
        {
            base.Spawned();
            timer = TickTimer.CreateFromTicks(Runner, ticksToExist);
            startTick = Runner.Tick;
            owner.GetBehaviour<RRoseMan>().currentScythe = Object;
        }

        public override void FixedUpdateNetwork()
        {
            if (timer.Expired(Runner))
            {
                boxManager.ResetAllBoxes();
                var no = Runner.Spawn(crPrefab, transform.position, transform.rotation, Object.InputAuthority, (runner, o) =>
                {
                    
                });
                owner.GetBehaviour<RRoseMan>().currentScythe = no;
                Runner.Despawn(Object, true);
                return;
            }

            int cExistTick = Runner.Tick - startTick;

            float t = (float)(cExistTick) / (float)(ticksToExist);
            
            force = transform.forward * fMoveForce * moveCurve.Evaluate(t);

            if (cExistTick % clearPer == 0)
            {
                Reset();
            }
            
            base.FixedUpdateNetwork();
        }
    }
}