using Fusion;
using UnityEngine;

namespace rwby.core
{
    public class BulletProjectile : BaseProjectile, ITargetingProjectile, IHomingProjectile
    {
        [Networked] public NetworkObject NetworkedTarget { get; set; }
        public NetworkObject Target
        {
            get => NetworkedTarget;
            set => NetworkedTarget = value;
        }
        public float AutoTargetStrength
        {
            get => autoTargetTurnSpeed;
            set => autoTargetTurnSpeed = value;
        }
        
        [Networked] public int startTick { get; set; }
        [Networked] public TickTimer timer { get; set; }
        public int ticksToExist = 200;

        public float autoTargetTurnSpeed = 1.0f;

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
                boxManager.ResetAllBoxes();
                Runner.Despawn(Object, true);
                return;
            }

            if (Target)
            {
                Vector3 directionToTarget = (Target.transform.position - transform.position).normalized;
                Vector3 currentDirection = transform.forward;
                Vector3 resultingDirection = Vector3.RotateTowards(currentDirection, directionToTarget, autoTargetTurnSpeed * Mathf.Deg2Rad * Time.deltaTime, 1f);
                transform.rotation = Quaternion.LookRotation(resultingDirection);
            }
            
            boxManager.ResetAllBoxes();
            boxManager.AddBox(cb.boxType, cb.attachedTo, cb.shape, cb.offset, cb.boxExtents, cb.radius, cb.definitionIndex, this);
            Move();
        }

        public override void Move()
        {
            transform.position += transform.forward * force.z * Runner.DeltaTime;
        }

        public override void DoHit(CustomHitbox hitbox, Hurtbox enemyHurtbox, HurtInfo hurtInfo)
        {
            base.DoHit(hitbox, enemyHurtbox, hurtInfo);
            Runner.Despawn(Object, true);
        }
    }
}