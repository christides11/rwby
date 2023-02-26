using System;
using UnityEngine;

namespace rwby
{
    public class ParticleSystemEffect : BaseEffect
    {
        public override float CurrentTime { get => particleSystems[0].time; protected set => SetFrame(value); }

        [SerializeField] protected ParticleSystem[] particleSystems;

        public override void SyncEffect(float wantedTime, bool pause = false)
        {
            if (wantedTime > duration)
            {
                PauseEffect();
                return;
            }
            if (pause)
            {
                PauseEffect();
                if (Mathf.Abs(CurrentTime - wantedTime) > (Time.fixedDeltaTime + 0.1f))
                {
                    SetFrame(wantedTime);
                }
                return;
            }
            switch (PlayState)
            {
                case EffectPlayState.STOPPED:
                    SetFrame(wantedTime);
                    PlayEffect(true, true);
                    break;
                case EffectPlayState.PLAYING:
                    if (Mathf.Abs(CurrentTime - wantedTime) > (Time.fixedDeltaTime + 0.1f))
                    {
                        SetFrame(wantedTime);
                        PlayEffect(false, true);
                    }
                    break;
                case EffectPlayState.PAUSED:
                    SetFrame(wantedTime);
                    PlayEffect(false, true);
                    break;
            }
        }

        public override void PlayEffect(bool restart = true, bool autoDelete = true)
        {
            if (PlayState == EffectPlayState.PLAYING && !restart) { return; }

            foreach (ParticleSystem p in particleSystems)
            {
                if(restart) p.Stop();
                p.Play();
            }

            PlayState = EffectPlayState.PLAYING;
        }

        public override void PauseEffect()
        {
            foreach(ParticleSystem p in particleSystems)
            {
                p.Pause(true);
            }
            PlayState = EffectPlayState.PAUSED;
        }
        public override void StopEffect(ParticleSystemStopBehavior stopBehavior)
        {
            for (int i = 0; i < particleSystems.Length; i++)
            {
                particleSystems[i].Stop(true, stopBehavior);
            }
        }

        public override void SetFrame(float time)
        {
            for (int i = 0; i < particleSystems.Length; i++)
            {
                if (!particleSystems[i].isPlaying || Mathf.Abs(particleSystems[i].time - time) > (1.5f / 60.0f))
                {
                    particleSystems[i].Simulate(time, true, true);
                }
            }
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