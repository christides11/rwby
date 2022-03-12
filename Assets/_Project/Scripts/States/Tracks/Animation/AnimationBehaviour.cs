using HnSF;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public struct AnimationEntry
    {
        public ModObjectReference animationbankReference;
        public int animation;
        public float startWeight;
        public float time;
    }
    
    [System.Serializable]
    public class AnimationBehaviour : FighterStateBehaviour
    {
        public ForceSetType animationSetType = ForceSetType.SET;
        public int layer;
        public AnimationEntry[] animations = new AnimationEntry[1];

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterAnimator animator = (playerData as FighterManager).fighterAnimator;
            animator.SyncFromState(animationSetType, layer, animations);
        }
    }
}