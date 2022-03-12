using HnSF;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class AnimationTimeBehaviour : FighterStateBehaviour
    {
        public int layer;
        public int index;
        public float time;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterAnimator animator = (playerData as FighterManager).fighterAnimator;
            animator.SetAnimationTime(layer, index, time);
        }
    }
}