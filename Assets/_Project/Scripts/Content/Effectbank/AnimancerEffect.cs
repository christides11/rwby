using Animancer;
using UnityEngine;


namespace rwby
{
    public class AnimancerEffect : BaseEffect
    {
        public AnimancerComponent animancer;
        public AnimationClip clip;
        
        private void Awake()
        {
            animancer.Layers[0].Weight = 1.0f;
            animancer.Layers[0].Speed = 0;
            animancer.Layers[0].CreateIfNew(clip);
            animancer.Layers[0].GetOrCreateState(clip).Weight = 1.0f;
            animancer.Playable.PauseGraph();
        }

        public override void SetFrame(float time)
        {
            var s = animancer.Layers[0].GetOrCreateState(clip);
            s.Time = time;
            animancer.Evaluate();
        }
    }
}