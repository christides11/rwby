using HnSF;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class IncrementFrameAsset : FighterStateAsset
    {
        public IncrementFrameBehaviour template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<IncrementFrameBehaviour>.Create(graph, template);
            return playable;
        }
    }
}