using HnSF;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class DecrementHitstunAsset : FighterStateAsset
    {
        public DecrementHitstunBehaviour template;
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            template.timelineClip = clipPassthrough;
            var playable = ScriptPlayable<DecrementHitstunBehaviour>.Create(graph, template);
            return playable;
        }
    }
}