using UnityEngine;

namespace rwby
{
    public class BaseEffect : MonoBehaviour
    {
        [HideInInspector] public int bank;
        [HideInInspector] public int effect;
        
        public virtual void PlayEffect(bool restart = true, bool autoDelete = true)
        {

        }

        public virtual void SetFrame(float time)
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

        public virtual void SetRandomSeed(uint seed)
        {
            
        }
    }
}