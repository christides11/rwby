using UnityEngine;
using Fusion;

namespace rwby
{
    public class BaseEffect : NetworkBehaviour, IPredictedSpawnBehaviour
    {
        public virtual void PlayEffect(bool restart = true, bool autoDelete = true)
        {

        }

        public virtual void PauseEffect()
        {

        }

        public virtual void StopEffect(ParticleSystemStopBehavior stopBehavior)
        {

        }

        public virtual void DestroyEffect()
        {
            Runner.Despawn(Object, true);
        }

        public override void Spawned()
        {

        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {

        }

        public override void FixedUpdateNetwork()
        {

        }

        // PREDICTION //
        public virtual void PredictedSpawnSpawned()
        {

        }

        public virtual void PredictedSpawnUpdate()
        {

        }

        public virtual void PredictedSpawnRender()
        {

        }

        public virtual void PredictedSpawnSuccess()
        {

        }

        public virtual void PredictedSpawnFailed()
        {
            Runner.Despawn(Object, true);
        }
    }
}