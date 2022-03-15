using HnSF;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class AnimationTimeAsset : FighterStateAsset
    {
        [SerializeField] public AnimationTimeBehaviour template;
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<AnimationTimeBehaviour>.Create(graph, template);
            return playable;
        }
    }
}