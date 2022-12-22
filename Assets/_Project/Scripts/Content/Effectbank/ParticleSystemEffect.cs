using System;
using UnityEngine;

namespace rwby
{
    public class ParticleSystemEffect : BaseEffect
    {
        [SerializeField] protected ParticleSystem[] particleSystems;

        public override void SetFrame(float time)
        {
            for (int i = 0; i < particleSystems.Length; i++)
            {
                if (time > particleSystems[i].totalTime) continue;
                if (!particleSystems[i].isPlaying || Mathf.Abs(particleSystems[i].time - time) > (1.5f / 60.0f))
                {
                    particleSystems[i].Simulate(time, true, true);
                    particleSystems[i].Play(true);
                }
            }
        }

        public override void StopEffect(ParticleSystemStopBehavior stopBehavior)
        {
            /*
            for (int i = 0; i < particleSystems.Length; i++)
            {
                particleSystems[i].Stop(true, stopBehavior);
            }*/
        }
        

        public override void SetRandomSeed(uint seed)
        {
            for (int i = 0; i < particleSystems.Length; i++)
            {
                if (particleSystems[i].isPlaying) continue;
                particleSystems[i].randomSeed = seed;
            }
        }
    }
}