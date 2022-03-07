using HnSF;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class ClampGravityAsset : FighterStateAsset
    {
        public ClampGravityBehaviour template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ClampGravityBehaviour>.Create(graph, template);
            return playable;
        }
    }
}