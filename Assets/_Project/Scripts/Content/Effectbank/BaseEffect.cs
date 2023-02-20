using UnityEngine;

namespace rwby
{
    public class BaseEffect : MonoBehaviour
    {
        [HideInInspector] public int bank;
        [HideInInspector] public int effect;

        public EffectPlayState PlayState { get; protected set; } = EffectPlayState.STOPPED;
        public virtual float CurrentTime { get; protected set; } = 0;
        public float Duration { get => duration; protected set => duration = value; }

        [SerializeField] protected float duration = 0;

        public virtual void SyncEffect(float wantedTime, bool pause = false)
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
                    PlayEffect();
                    SetFrame(wantedTime);
                    break;
                case EffectPlayState.PLAYING:
                    if (Mathf.Abs(CurrentTime - wantedTime) > (Time.fixedDeltaTime + 0.1f))
                    {
                        SetFrame(wantedTime);
                    }
                    break;
                case EffectPlayState.PAUSED:
                    PlayEffect();
                    SetFrame(wantedTime);
                    break;
            }
        }

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