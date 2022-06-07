using UnityEngine;
using Fusion;

namespace rwby
{
    public class BaseEffect : MonoBehaviour
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
            
        }
    }
}