using HnSF;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class GotoFrameAsset : FighterStateAsset
    {
        public GotoFrameBehaviour template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<GotoFrameBehaviour>.Create(graph, template);
            return playable;
        }
    }
}