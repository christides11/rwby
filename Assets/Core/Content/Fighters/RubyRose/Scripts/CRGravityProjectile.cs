using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby.core
{
    public class CRGravityProjectile : BaseProjectile
    {
        [Networked] public int startTick { get; set; }

        public int clearPer = 2;

        public NetworkObject crPrefab;

        public float gravityForce = 9.81f;
        public float maxGravity = 20f;
        
        private void Awake()
        {
            boxManager.hurtable = this;
        }

        public override void Spawned()
        {
            base.Spawned();
            startTick = Runner.Tick;
            owner.GetBehaviour<RRoseMan>().currentScythe = Object;
        }

        private Collider[] groundResultList = new Collider[1];
        public float colExtents = 0.5f;
        public LayerMask groundedLayerMask;
        public override void FixedUpdateNetwork()
        {
            var overlapCnt = Runner.GetPhysicsScene().OverlapBox(transform.position, new Vector3(colExtents, colExtents, colExtents), groundResultList,
                transform.rotation, groundedLayerMask);
            if (overlapCnt > 0)
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
            
            var f = force;
            f.y = Mathf.Clamp(f.y - (gravityForce * Runner.DeltaTime), -maxGravity, float.MaxValue);
            force = f;
            

            if (cExistTick % clearPer == 0)
            {
                Reset();
            }
            
            base.FixedUpdateNetwork();
        }
    }
}