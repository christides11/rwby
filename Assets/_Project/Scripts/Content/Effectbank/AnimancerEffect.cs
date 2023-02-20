using Animancer;
using UnityEngine;


namespace rwby
{
    public class AnimancerEffect : BaseEffect
    {
        public override float CurrentTime { get => animancer.Layers[0].GetOrCreateState(clip).Time; protected set => animancer.Layers[0].GetOrCreateState(clip).Time = value; }

        public AnimancerComponent animancer;
        public AnimationClip clip;
        
        private void Awake()
        {
            /*
            animancer.Playable.PauseGraph();
            animancer.Layers[0].Weight = 1.0f;
            //animancer.Layers[0].Speed = 0;
            animancer.Layers[0].CreateIfNew(clip);
            animancer.Layers[0].GetOrCreateState(clip).Weight = 1.0f;*/
        }

        private void LateUpdate()
        {
            if (PlayState == EffectPlayState.PLAYING && CurrentTime > duration)
            {
                PauseEffect();
                return;
            }
        }

        public override void SyncEffect(float wantedTime, bool pause = false)
        {
            if(wantedTime > duration)
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

        public override void PlayEffect(bool restart = true, bool autoDelete = true)
        {
            if (PlayState == EffectPlayState.PLAYING)
            {
                if (!restart) return;
                animancer.Play(clip).Time = 0;
            }
            if(PlayState == EffectPlayState.PAUSED)
            {
                animancer.Playable.UnpauseGraph();
            }
            else
            {
                animancer.Play(clip).Time = 0;
            } 
            PlayState = EffectPlayState.PLAYING;
        }

        public override void PauseEffect()
        {
            animancer.Playable.PauseGraph();
            PlayState = EffectPlayState.PAUSED;
        }

        public override void StopEffect(ParticleSystemStopBehavior stopBehavior)
        {
            animancer.Stop(clip);
            PlayState = EffectPlayState.STOPPED;
            CurrentTime = 0;
        }

        public override void SetFrame(float time)
        {
            animancer.Layers[0].GetOrCreateState(clip).Time = time;
            /*
            var s = animancer.Layers[0].GetOrCreateState(clip);
            s.Time = time;
            animancer.Evaluate();*/
        }
    }
}