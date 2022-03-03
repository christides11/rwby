using HnSF;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class GravityAsset : FighterStateAsset
    {
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<GravityBehaviour>.Create(graph);
            return playable;
        }
    }
}