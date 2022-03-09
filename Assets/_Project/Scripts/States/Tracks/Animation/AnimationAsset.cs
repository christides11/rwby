using HnSF;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class AnimationAsset : FighterStateAsset
    {
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<AnimationBehaviour>.Create(graph);
            return playable;
        }
    }
}