using UnityEngine;

namespace rwby
{
    public class ParticleSystemEffect : BaseEffect
    {
        
        [SerializeField] protected ParticleSystem[] particleSystems;

        public override void SetFrame(float time)
        {
            base.SetFrame(time);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                particleSystems[i].Simulate(time, true, true);
            }
        }
    }
}