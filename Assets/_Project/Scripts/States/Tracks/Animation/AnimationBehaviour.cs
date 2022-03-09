using HnSF.Sample.TDAction;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class AnimationBehaviour : FighterStateBehaviour
    {
        [System.Serializable]
        public struct AnimationEntry
        {
            public float weight;
            public float normalizedTime;
            public ModObjectReference animationbankReference;
            public int animation;
        }
        
        public int layer;
        public AnimationMixerType animationMixerType;
        public Vector2 mixerPosition;
        public AnimationEntry[] animations = new AnimationEntry[1];

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);
            
        }
    }
}