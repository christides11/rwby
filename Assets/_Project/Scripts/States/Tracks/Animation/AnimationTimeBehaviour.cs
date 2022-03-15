using HnSF;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class AnimationTimeBehaviour : FighterStateBehaviour
    {
        public bool timeInFrames = false;
        public int layer;
        public int index;
        public float time;
        public int frameTime;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterAnimator animator = (playerData as FighterManager).fighterAnimator;
            animator.SetAnimationTime(layer, index, timeInFrames ? frameTime * animator.Runner.DeltaTime : time);
        }
    }
}